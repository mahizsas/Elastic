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

namespace mpbdmService.Controllers
{
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

        private string findShard()
        {
            w.Start();
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            Guid shardKey = new Guid(currentUser.Id.Split((":").ToArray<char>()).FirstOrDefault().ToString());
            db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, shardKey, WebApiConfig.ShardingObj.connstring);

            ((EntityDomainManager<Companies>)DomainManager).Context = db;
            return shardKey.ToString();
        }
        Stopwatch w = new Stopwatch(); 
        // GET tables/Companies
        public IQueryable<Companies> GetAllCompanies()
        {
            string shardKey = findShard();  
            return Query().Where(s=>s.Id == shardKey);
        }

        // GET tables/Companies/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Companies> GetCompanies(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Companies/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Companies> PatchCompanies(string id, Delta<Companies> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Companies
        public async Task<IHttpActionResult> PostCompanies(Companies item)
        {
            Companies current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Companies/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteCompanies(string id)
        {
            return DeleteAsync(id);
        }
    }
}