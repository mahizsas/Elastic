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
using System.Diagnostics;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using mpbdmService.ElasticScale;
using System.Net.Http;
using System.Net;

namespace mpbdmService.Controllers
{
    [RoutePrefix("tables/Companies")]
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class CompaniesController : TableController<Companies>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            DomainManager = new EntityDomainManager<Companies>(db, Request, Services);
        }
        private mpbdmContext<Guid> db;

        /*
         * Dont be misleading it get the shardKey we need on each request
         * BUT sets the DomainManager's context to look at the correct shard
         */
        private string getShardKey()
        {
            string shardKey = Sharding.FindShard(User);
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            ((EntityDomainManager<Companies>)DomainManager).Context = db;
            return shardKey;
        }
        // GET tables/Companies
        public IQueryable<Companies> GetAllCompanies()
        {
            string shardKey = getShardKey();
            return Query().Where(s=>s.Id == shardKey);
        }
        
        // PATCH tables/Companies/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Companies> PatchCompanies(string id, Delta<Companies> patch)
        {
            getShardKey();
            return UpdateAsync(id, patch);
        }

        // POST tables/Companies
        public async Task<IHttpActionResult> PostCompanies(Companies item)
        {
            getShardKey();
            Companies current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Companies/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteCompanies(string id)
        {
            getShardKey();
            return DeleteAsync(id);
        }


        [Route("coins")]
        public HttpResponseMessage GetCoins()
        {

            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);

            var coins = getRecieverMoney(shardKey, db);
            return Request.CreateResponse(HttpStatusCode.OK, coins);
        }
        public static double getRecieverMoney(string shardKey, mpbdmContext<Guid> db)
        {
            var res = db.Transactions.Where(s => s.Status == "Completed" && (s.Sender == shardKey || s.Reciever == shardKey)).OrderByDescending(s => s.UpdatedAt).FirstOrDefault();
            if (res != null)
            {
                if (res.Reciever == shardKey) return res.RecieverMoney;
                else return res.SenderMoney;
            }
            return 0.0;
        }
    }
}