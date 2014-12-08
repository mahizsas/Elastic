﻿using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using mpbdmService.DomainManager;

namespace mpbdmService.Controllers
{
    public class MobileFavoriteController : TableController<MobileFavorites>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            mpbdmContext context = new mpbdmContext();
            DomainManager = new FavoritesDomainManager(context, Request, Services , User);
        }

        // GET tables/MobileFavorites
        public IQueryable<MobileFavorites> GetAllMobileFavorites()
        {
            ((FavoritesDomainManager)DomainManager).User = User;
            return Query(); 
        }

        // GET tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<MobileFavorites> GetMobileFavorites(string id)
        {
            ((FavoritesDomainManager)DomainManager).User = User;
            return Lookup(id);
        }

        // PATCH tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<MobileFavorites> PatchMobileFavorites(string id, Delta<MobileFavorites> patch)
        {
            ((FavoritesDomainManager)DomainManager).User = User;
             return UpdateAsync(id, patch);
        }

        // POST tables/MobileFavorites
        public async Task<IHttpActionResult> PostMobileFavorites(MobileFavorites item)
        {
            ((FavoritesDomainManager)DomainManager).User = User;
            MobileFavorites current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/MobileFavorites/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteMobileFavorites(string id)
        {
            ((FavoritesDomainManager)DomainManager).User = User;
             return DeleteAsync(id);
        }


    }
}