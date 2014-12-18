using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using mpbdmService.DomainManager;
using System;
using mpbdmService.ElasticScale;

namespace mpbdmService.Controllers
{
    public class MobileFavoriteController : TableController<MobileFavorites>
    {
        private mpbdmContext<Guid> db;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            DomainManager = new FavoritesDomainManager(db, Request, Services , User);
        }
        private string getShardKey()
        {
            string shardKey = Sharding.FindShard(User);
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            ((FavoritesDomainManager)DomainManager).setContext(db);
            ((FavoritesDomainManager)DomainManager).User = User;
            return shardKey;
        }

        // GET tables/MobileFavorites
        public IQueryable<MobileFavorites> GetAllMobileFavorites()
        {
            getShardKey();
            return Query(); 
        }

        // GET tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<MobileFavorites> GetMobileFavorites(string id)
        {
            getShardKey();
            return Lookup(id);
        }

        // PATCH tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<MobileFavorites> PatchMobileFavorites(string id, Delta<MobileFavorites> patch)
        {
            getShardKey();
            return UpdateAsync(id, patch);
        }

        // POST tables/MobileFavorites
        public async Task<IHttpActionResult> PostMobileFavorites(MobileFavorites item)
        {
            getShardKey();
            MobileFavorites current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMobileFavorites(string id)
        {
            getShardKey();
            return DeleteAsync(id);
        }


    }
}