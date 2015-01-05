using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using System;
using mpbdmService.ElasticScale;
using mpbdmService.DTO;
using mpbdmService.DomainManager;
using System.Net.Http;

namespace mpbdmService.Controllers
{
    public class MobileTransactionsController : TableController<MobileTransactions>
    {
        private mpbdmContext<Guid> db;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            DomainManager = new TransactionsDomainManager(db, Request, Services);
        }
        // GET tables/Transactions
        public IOrderedQueryable<MobileTransactions> GetAllTransactions()
        {
            ((TransactionsDomainManager)DomainManager).User = User; 
            return Query().OrderByDescending(s=>s.UpdatedAt);
        }
        // GET tables/Transactions/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<MobileTransactions> GetTransactions(string id)
        {
            string shardKey = Sharding.FindShard(User);
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            ((EntityDomainManager<MobileTransactions>)DomainManager).Context = db;
            return Lookup(id);
        }


        
    }
}