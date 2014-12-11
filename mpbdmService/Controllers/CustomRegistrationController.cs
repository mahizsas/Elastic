using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using mpbdmService.Models;
using System.Text.RegularExpressions;
using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using System.Data;
using mpbdmService.ElasticScale;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomRegistrationController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/CustomRegistration
        public HttpResponseMessage Post(RegistrationRequest registrationRequest)
        {
            if (!Regex.IsMatch(registrationRequest.email, "^([a-zA-Z0-9]{1,})@([a-z]{2,}).[a-z]{2,}$"))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid email!");
            }
            else if (registrationRequest.password.Length < 8)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid password (at least 8 chars required)");
            }

            // MUST FIND COMPANY BY EMAIL
            // CREATE a MULTISHARD COMMAND
            // SEARCH BY EMAIL
            mpbdmContext<Guid> context = null; 
            Guid shardKey;
            using (MultiShardConnection conn = new MultiShardConnection(WebApiConfig.ShardingObj.ShardMap.GetShards(),WebApiConfig.ShardingObj.connstring ))
            {
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    // Get emailDomain 
                    char[] papaki = new char[1];
                    papaki[0] = '@';
                    // SQL INJECTION SECURITY ISSUE
                    string emailDomain = registrationRequest.email.Split(papaki).Last();

                    // CHECK SCHEMA
                    cmd.CommandText = "SELECT Id FROM [mpbdm].[Companies] WHERE Email LIKE '%" + emailDomain + "'";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecutionOptions = MultiShardExecutionOptions.IncludeShardNameColumn;
                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.PartialResults;
                    
                    using (MultiShardDataReader sdr = cmd.ExecuteReader())
                    {
                        bool res = sdr.Read();
                        if (res != false)
                        {
                            shardKey = new Guid(sdr.GetString(0));
                        }
                        else
                        {
                            if (registrationRequest.CompanyName == null || registrationRequest.CompanyAddress == null)
                            {
                                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Company under this email domain doesn't exist! To create a company with your registration please provide CompanyName and CompanyAddress parameters");
                            }


                            Companies comp = new Companies();
                            comp.Id = Guid.NewGuid().ToString();

                            comp.Name = registrationRequest.CompanyName;
                            comp.Address = registrationRequest.CompanyAddress;
                            comp.Email = registrationRequest.email;
                            comp.Deleted = false;

                            // SHARDING Find where to save the new company
                            Shard shard = WebApiConfig.ShardingObj.FindRoomForCompany();
                            WebApiConfig.ShardingObj.RegisterNewShard(shard.Location.Database , comp.Id);
                            //Connect to the db registered above
                            shardKey = new Guid(comp.Id);
                            context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, shardKey, WebApiConfig.ShardingObj.connstring);
                            // Add to the db
                            context.Companies.Add(comp);
                            context.SaveChanges();
                        }
                    }
                }
            }
            //////////////////////////////////////////////////////////////////////

            // MUST RECHECK CORRECT DB!!!!!!!!!!!
            if( context == null )
                context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, shardKey, WebApiConfig.ShardingObj.connstring);

            Account account = context.Accounts.Where(a => a.User.Email == registrationRequest.email).SingleOrDefault();
            if (account != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Email already exists");
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.generateSalt();

                string compId = shardKey.ToString();

                Users newUser = new Users
                {
                    Id = CustomLoginProvider.ProviderName + ":" + registrationRequest.email,
                    CompaniesID = compId,
                    FirstName = registrationRequest.firstName,
                    LastName = registrationRequest.lastName,
                    Email = registrationRequest.email
                };
                
                Account newAccount = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    //Username = registrationRequest.username,
                    Salt = salt,
                    SaltedAndHashedPassword = CustomLoginProviderUtils.hash(registrationRequest.password, salt),
                    User = newUser
                };

                context.Users.Add(newUser);
                context.Accounts.Add(newAccount);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    var a = ex.InnerException;
                }
                return this.Request.CreateResponse(HttpStatusCode.Created);
            }
        }
    }   
}