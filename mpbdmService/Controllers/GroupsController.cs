using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DomainManager;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class GroupsController : TableController<Groups>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<string>();
            DomainManager = new EntityDomainManager<Groups>(db, Request, Services );
        }
        mpbdmContext<string> db;


        // GET tables/Groups
        public IQueryable<Groups> GetAllGroups()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            IQueryable<Groups> groups = from c in db.Groups
                                        join a in
                                            (from d in db.Companies
                                             join e in db.Users
                                             on d.Id equals e.CompaniesID
                                             where e.Id == currentId
                                             select d)
                                        on c.CompaniesID equals a.Id
                                        select c;
            return groups;

            //return Query() ;
        }

        // GET tables/Groups/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Groups> GetGroups(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Groups/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Groups> PatchGroups(string id, Delta<Groups> patch)
        {
            Groups item = patch.GetEntity();
            bool b = item.Visible;
            return UpdateAsync(id, patch);
        }

        // POST tables/Groups
        public async Task<IHttpActionResult> PostGroups(Groups item)
        {
            Groups current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Groups/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteGroups(string id)
        {
            return DeleteAsync(id);
        }
    }
}