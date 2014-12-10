using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;

namespace mpbdmService.Controllers
{
    //[AuthorizeLevel(AuthorizationLevel.User)] 
    public class UsersController : TableController<Users>
    {
        private mpbdmContext<Guid> db;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            DomainManager = new EntityDomainManager<Users>(db, Request, Services);
        }
        

        // GET tables/Users
        public IQueryable<Users> GetAllUsers()
        {
            var currentId = "Google:105535740556221909032";
            //var currentUser = User as ServiceUser;
            //var currentId = currentUser.Id;
            return Query().Where(user => user.Id == currentId); 
        }

        // GET tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Users> GetUsers(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Users> PatchUsers(string id, Delta<Users> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Users
        public async Task<IHttpActionResult> PostUsers(Users item)
        {
            Users current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUsers(string id)
        {
            return DeleteAsync(id);
        }
    }
}