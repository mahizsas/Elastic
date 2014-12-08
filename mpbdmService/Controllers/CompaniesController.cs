using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class CompaniesController : TableController<Companies>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            mpbdmContext context = new mpbdmContext();
            DomainManager = new EntityDomainManager<Companies>(context, Request, Services);
        }
        mpbdmContext db = new mpbdmContext();

        // GET tables/Companies
        public IQueryable<Companies> GetAllCompanies()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            IQueryable<Companies> companies = from c in db.Companies
                                            join a in db.Users
                                            on c.Id equals a.CompaniesID
                                            where a.Id == currentId
                                            select c;
            return companies;
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