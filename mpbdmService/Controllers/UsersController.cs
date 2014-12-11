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
using mpbdmService.ElasticScale;

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

        private string getShardKey()
        {
            string shardKey = Sharding.FindShard(User);
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            ((EntityDomainManager<Users>)DomainManager).Context = db;
            return shardKey;
        }
        // GET tables/Users
        public IQueryable<Users> GetAllUsers()
        {
            getShardKey();
            var cuser = User as ServiceUser;
            return Query().Where(user => user.Id == cuser.Id); 
        }

        // GET tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Users> GetUsers(string id)
        {
            getShardKey();
            return Lookup(id);
        }

        // PATCH tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Users> PatchUsers(string id, Delta<Users> patch)
        {
            getShardKey();
            return UpdateAsync(id, patch);
        }

        // POST tables/Users
        public async Task<IHttpActionResult> PostUsers(Users item)
        {
            getShardKey();
            Users current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Users/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUsers(string id)
        {
            getShardKey();
            return DeleteAsync(id);
        }
    }
}