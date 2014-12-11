using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DTO;
using mpbdmService.Models;
using mpbdmService.DomainManager;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using mpbdmService.ElasticScale;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class MobileGroupController : TableController<MobileGroup>
    {
        private mpbdmContext<Guid> db;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            this.DomainManager = new GroupsDomainManager(db, Request, Services);
        }

        private string getShardKey()
        {
            string shardKey = Sharding.FindShard(User);
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            ((GroupsDomainManager)DomainManager).Context = db;
            ((GroupsDomainManager)DomainManager).User = User;
            return shardKey;
        }

        // GET tables/MobileGroup
        public IQueryable<MobileGroup> GetAllMobileGroup()
        {
            getShardKey();
            return Query(); 
        }

        // GET tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<MobileGroup> GetMobileGroup(string id)
        {
            getShardKey();
            return Lookup(id);
        }

        // PATCH tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<MobileGroup> PatchMobileGroup(string id, Delta<MobileGroup> patch)
        {
            getShardKey();
            return UpdateAsync(id, patch);
        }

        // POST tables/MobileGroup
        public async Task<IHttpActionResult> PostMobileGroup(MobileGroup item)
        {
            getShardKey();
            MobileGroup current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMobileGroup(string id)
        {
            getShardKey();
            return DeleteAsync(id);
        }

    }
}