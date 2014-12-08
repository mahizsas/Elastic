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

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class MobileGroupController : TableController<MobileGroup>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            mpbdmContext context = new mpbdmContext();
            this.DomainManager = new GroupsDomainManager(context, Request, Services);
        }



        // GET tables/MobileGroup
        public IQueryable<MobileGroup> GetAllMobileGroup()
        {
            ((GroupsDomainManager)DomainManager).User = User;
            return Query(); 
        }

        // GET tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<MobileGroup> GetMobileGroup(string id)
        {
            ((GroupsDomainManager)DomainManager).User = User;
            return Lookup(id);
        }

        // PATCH tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<MobileGroup> PatchMobileGroup(string id, Delta<MobileGroup> patch)
        {
            ((GroupsDomainManager)DomainManager).User = User;
            return UpdateAsync(id, patch);
        }

        // POST tables/MobileGroup
        public async Task<IHttpActionResult> PostMobileGroup(MobileGroup item)
        {
            ((GroupsDomainManager)DomainManager).User = User;
            MobileGroup current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/MobileGroup/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMobileGroup(string id)
        {
            ((GroupsDomainManager)DomainManager).User = User;
            return DeleteAsync(id);
        }

    }
}