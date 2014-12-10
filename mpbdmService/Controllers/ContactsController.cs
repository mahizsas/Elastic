
using mpbdmService.DataObjects;
using mpbdmService.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json.Linq;

namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)] 
    public class ContactsController : TableController<Contacts>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            db = new mpbdmContext<Guid>();
            DomainManager = new EntityDomainManager<Contacts>(db, Request, Services , true);
            
        }
        private mpbdmContext<Guid> db;

        private string GetCompanyId(string currentId)
        {
            var user = db.Users.Where(s => s.Id == currentId).FirstOrDefault();
            if (user == null) return null;
            return user.CompaniesID;
        }

        // GET tables/Contacts
        public IQueryable<Contacts> GetAllContacts()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            //IQueryable<Contacts> contacts = from f in db.Contacts
            //                                join g in
            //                                    (from c in db.Groups
            //                                     join a in
            //                                         (from d in db.Companies
            //                                          join e in db.Users
            //                                          on d.Id equals e.CompaniesID
            //                                          where e.Id == currentId
            //                                          select d)
            //                                     on c.CompaniesID equals a.Id
            //                                     select c)
            //                            on f.GroupsID equals g.Id
            //                            select f;
            var cid = GetCompanyId(currentId);
            IQueryable<Contacts>  contacts = Query().Where(s => s.Groups.CompaniesID == cid);
            return contacts;
        }

        // GET tables/Contacts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Contacts> GetContacts(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Contacts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public  Task<Contacts> PatchContacts(string id, Delta<Contacts> patch)
        {
            //async
            return UpdateAsync(id, patch);

            //Services.Log.Info("PatchRoundTrip:" + id);
            //const int NumAttempts = 5;
            //HttpResponseException lastException = null;
           
            //for (int i = 0; i < NumAttempts; i++) {
            //    try
            //    {
            //        return await UpdateAsync(id, patch);
            //    }
            //    catch (HttpResponseException ex)
            //    {
            //        throw new HttpResponseException(this.Request.CreateNotFoundResponse());

            //        lastException = ex;
            //    }


            //    if (lastException.Response != null && lastException.Response.StatusCode == HttpStatusCode.PreconditionFailed) {
                    
            //        // Handle conflict
            //        var content = lastException.Response.Content as ObjectContent;
            //        Contacts serverItem = (Contacts)content.Value;
                    
            //        //KeyValuePair<string, string> kvp = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => p.Key == "conflictPolicy");
            //        //if (kvp.Key == "conflictPolicy") {
            //            //switch (kvp.Value) {
            //                //case "clientWins":
                   
            //                    this.Request.Headers.IfMatch.Clear();
            //                    this.Request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"" + Convert.ToBase64String(serverItem.Version) + "\""));
            //                    continue; // try again with the new ETag...
            //                //case "serverWins":
            //                    //Services.Log.Info("Server wins");
            //                    //return serverItem;
            //            //}
            //        //}
            //    }
            //    throw lastException;
            //}

            //throw lastException;
        }

        // POST tables/Contacts
        public async Task<IHttpActionResult> PostContacts(Contacts item)
        {
            if (item.GroupsID == null)
            {
                return null;
            }
            Contacts current = await InsertAsync(item);
            
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Contacts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteContacts(string id)
        {
            return this.DeleteAsync(id);
        }
    }
}