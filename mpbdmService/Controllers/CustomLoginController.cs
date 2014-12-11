using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using mpbdmService;
using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using System.Data;
namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CustomLoginController : ApiController
    {
        public ApiServices Services { get; set; }
        public IServiceTokenHandler handler { get; set; }

        // POST api/CustomLogin
        public HttpResponseMessage Post(LoginRequest loginRequest)
        {

            Guid shardKey;
            // SEND A QUERY TO ALL SHARD TO DETECT OUR SHARD!!!!
            // SAVE companiesId to shardKey!
            using (MultiShardConnection conn = new MultiShardConnection(WebApiConfig.ShardingObj.ShardMap.GetShards(), WebApiConfig.ShardingObj.connstring))
            {
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    // CHECK SCHEMA
                    // SQL INJECTION SECURITY ISSUE
                    cmd.CommandText = "SELECT CompaniesID FROM [mpbdm].[Accounts] JOIN [mpbdm].[Users] ON [mpbdm].[Users].Id = [mpbdm].[Accounts].User_Id WHERE email='" + loginRequest.email + "'";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecutionOptions = MultiShardExecutionOptions.IncludeShardNameColumn;
                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.PartialResults;
                    // Async
                    using (MultiShardDataReader sdr = cmd.ExecuteReader())
                    {
                        bool res = sdr.Read();
                        if( res != false ){
                            shardKey = new Guid(sdr.GetString(0));
                        }
                        else
                        {
                            return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Account doesn't exist!");
                        }
                    }
                }
            }
            // Connect with entity framework to the specific shard
            mpbdmContext<Guid> context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, shardKey , WebApiConfig.ShardingObj.connstring);
            Account account = context.Accounts.Include("User").Where(a => a.User.Email == loginRequest.email).SingleOrDefault();
            if (account != null)
            {
                byte[] incoming = CustomLoginProviderUtils.hash(loginRequest.password, account.Salt);

                if (CustomLoginProviderUtils.slowEquals(incoming, account.SaltedAndHashedPassword))
                {
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, account.Username));
                    // Custom Claim must be added to CustomLoginProvider too !! 
                    claimsIdentity.AddClaim(new Claim("shardKey", account.User.CompaniesID));
                    var  customLoginProvider = new CustomLoginProvider(handler);
                    LoginResult loginResult = customLoginProvider.CreateLoginResult(claimsIdentity, Services.Settings.MasterKey);
                    MobileLoginResult res = new MobileLoginResult(account, loginResult);
                    return this.Request.CreateResponse(HttpStatusCode.OK, res);
                }                                                        
            }
            return this.Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid username or password");
        }

    }
}