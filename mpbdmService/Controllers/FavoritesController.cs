using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Net;
using System;
using System.Net.Http;

namespace mpbdmService.Controllers
{
    //[AuthorizeLevel(AuthorizationLevel.User)] 
    public class FavoritesController : TableController<Favorites>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            mpbdmContext context = new mpbdmContext();
            DomainManager = new EntityDomainManager<Favorites>(context, Request, Services);
        }
        mpbdmContext db = new mpbdmContext();

        // GET tables/Favorites
        public IQueryable<Favorites> GetAllFavorites()
        {
            //HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.NotFound)
            //{
            //    Content = new StringContent("Duplicate"),
            //    ReasonPhrase = "ContactsID Found two times in Database!"
            //};
            //throw new HttpResponseException(msg);

            //var currentUser = User as ServiceUser;
            //var currentId = currentUser.Id;
            var currentId = "Google:105535740556221909032";
            IQueryable<Favorites> favorites= from c in db.Favorites
                                                        join b in db.Contacts
                                                      on c.ContactsID equals b.Id
                                                       where c.UsersID == currentId 
                                                        select c;
            return favorites;
        }

        // GET tables/ToItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Favorites> GetFavorites(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Favorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Favorites> PatchFavorites(string id, Delta<Favorites> patch)
        {
            
            return UpdateAsync(id, patch);
        }

        // POST tables/Favorites
        public async Task<IHttpActionResult> PostFavorites(Favorites item)
        {
            IQueryable<Favorites> favorites;
            var current_cont_id = item.ContactsID;
            favorites = from c in db.Favorites
                                              join b in db.Contacts
                                              on c.ContactsID equals b.Id
                                              where current_cont_id == b.Id
                                              select c;

            var counter = favorites.Count();
            if (counter>0)
            {

                //HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.NotFound)
                //{
                //    Content = new StringContent("Duplicate"),
                //    ReasonPhrase = "ContactsID Found two times in Database!"
                //};
                //throw new HttpResponseException(msg);

                return BadRequest(); 
            }
            else {
                Favorites current = await InsertAsync(item);
                return CreatedAtRoute("Tables", new { id = current.Id }, current);
            }
            
        }

        // DELETE tables/Favorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteFavorites(string id)
        {
            return DeleteAsync(id);
        }
    }
}