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
            // MUST RECHECK CORRECT DB!!!!!!!!!!!
            mpbdmContext<string> context = new mpbdmContext<string>();
            Account account = context.Accounts.Where(a => a.Username == registrationRequest.username).SingleOrDefault();
            if (account != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Username already exists");
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.generateSalt();

                string compId = "2c8c7462-d6ca-429c-9021-21203bea780d";
                Users newUser = new Users
                {
                    Id = CustomLoginProvider.ProviderName + ":" + registrationRequest.username,
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
                
                context.Accounts.Add(newAccount);
                context.SaveChanges();
                return this.Request.CreateResponse(HttpStatusCode.Created);
            }
        }
    }   
}