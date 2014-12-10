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

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomRegistrationController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/CustomRegistration
        public HttpResponseMessage Post(RegistrationRequest registrationRequest)
        {
            if (!Regex.IsMatch(registrationRequest.username, "^[a-zA-Z0-9]{4,}$"))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid username (at least 4 chars, alphanumeric only)");
            }
            else if (registrationRequest.password.Length < 8)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid password (at least 8 chars required)");
            }

            // MUST FIND COMPANY BY EMAIL
            // CREATE a MULTISHARD COMMAND
            // SEARCH BY EMAIL
            Guid shardKey;
            var aa = WebApiConfig.ShardingObj.ShardMap.GetShards();
            var ba = WebApiConfig.ShardingObj.connstring;
            using (MultiShardConnection conn = new MultiShardConnection(aa,ba ))
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
                            //var a = sdr.GetBytes(0, 0, salt, 0, 256); // 256 comes from cryptography
                            //var b = sdr.GetBytes(1, 0, saltPass, 0, 512); // 512comes from cryptography
                            shardKey = new Guid(sdr.GetString(0));
                        }
                        else
                        {
                            return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Registration failed propably there is no Company with such email!");
                        }
                    }
                }
            }
            //////////////////////////////////////////////////////////////////////

            // MUST RECHECK CORRECT DB!!!!!!!!!!!
            mpbdmContext<Guid> context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, shardKey, WebApiConfig.ShardingObj.connstring);
            Account account = context.Accounts.Where(a => a.Username == registrationRequest.username).SingleOrDefault();
            if (account != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Username already exists");
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.generateSalt();

                string compId = shardKey.ToString();
                Users newUser = new Users
                {
                    Id = shardKey + ":" + registrationRequest.username,
                    CompaniesID = compId,
                    FirstName = registrationRequest.firstName,
                    LastName = registrationRequest.lastName,
                    Email = registrationRequest.email
                };
                
                Account newAccount = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = registrationRequest.username,
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