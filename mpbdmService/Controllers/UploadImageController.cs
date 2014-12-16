using azmobService.Photo;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Configuration;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.ElasticScale;


namespace mpbdmService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class UploadImageController : ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {

            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            // Security issue check company
            var user = User as ServiceUser;
            Users userEntity = db.Set<Users>().Where(s => s.Id == user.Id).FirstOrDefault();
            if (userEntity == null)
            {
                this.Request.CreateResponse(HttpStatusCode.BadRequest, "User doesnt't exist!");
            }

            CloudStorageAccount acc = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Azure"].ConnectionString);
            CloudBlobClient blobClient = acc.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference("images");
            await photoContainer.CreateIfNotExistsAsync();
            var provider = new AzureBlobMultipartFormDataStreamProvider(photoContainer);
            await this.Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.FileData)
            {
                var blob = await photoContainer.GetBlobReferenceFromServerAsync(file.LocalFileName);
                var fileNameGuid = Guid.NewGuid().ToString();
                ICloudBlob newBlob = null;
                if (blob is CloudBlockBlob)
                {
                    newBlob = photoContainer.GetBlockBlobReference(fileNameGuid);
                }
                else
                {
                    newBlob = photoContainer.GetPageBlobReference(fileNameGuid);
                }
                await newBlob.StartCopyFromBlobAsync(blob.Uri);
                blob.Delete();
                await newBlob.FetchAttributesAsync();
                string url = newBlob.Uri.ToString();

                //// DELETING ANY OLD BLOBS
                //if (userEntity.ImageUrl != null)
                //{
                //    var oldBlob = photoContainer.GetBlobReferenceFromServer(userEntity.ImageUrl);
                //    oldBlob.Delete();
                //}
                ////////////////////////////
                // UPDATE imageUrl of user
                userEntity.ImageUrl = url;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "CannotSaveChanges!");
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        public async Task<HttpResponseMessage> Post(string contactId)
        {
            
            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            // Security issue check company
            Contacts contact = db.Set<Contacts>().Include("Groups").Where(s=>s.Id == contactId && s.Groups.CompaniesID == shardKey ).FirstOrDefault();
            if( contact == null ){
                this.Request.CreateResponse(HttpStatusCode.BadRequest,"Contact doesnt't exist!");
            }

            CloudStorageAccount acc = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Azure"].ConnectionString);
            CloudBlobClient blobClient = acc.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference("images");

            await photoContainer.CreateIfNotExistsAsync();

            var provider = new AzureBlobMultipartFormDataStreamProvider(photoContainer);

            await this.Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in provider.FileData)
            {
                //the LocalFileName is going to be the absolute Uri of the blob (see GetStream)
                //use it to get the blob info to return to the client
                var blob = await photoContainer.GetBlobReferenceFromServerAsync(file.LocalFileName);
                var fileNameGuid = Guid.NewGuid().ToString();
                // Copy to get new URL
                ICloudBlob newBlob = null;
                if (blob is CloudBlockBlob)
                {
                    newBlob = photoContainer.GetBlockBlobReference(fileNameGuid);
                }
                else
                {
                    newBlob = photoContainer.GetPageBlobReference(fileNameGuid);
                }
                //Initiate blob copy
                await newBlob.StartCopyFromBlobAsync(blob.Uri);
                ////Now wait in the loop for the copy operation to finish
                //while (true)
                //{
                //    newBlob.FetchAttributes();
                //    if (newBlob.CopyState.Status != CopyStatus.Pending)
                //    {
                //        break;
                //    }
                //    //Sleep for a second may be
                //    System.Threading.Thread.Sleep(1000);
                //}
                blob.Delete();

                await newBlob.FetchAttributesAsync();

                string url = newBlob.Uri.ToString();
                //// DELETING ANY OLD BLOBS
                //if (contact.ImageUrl != null)
                //{
                //    var oldBlob = photoContainer.GetBlobReferenceFromServer(contact.ImageUrl);
                //    oldBlob.Delete();
                //}
                ////////////////////////////
                contact.ImageUrl = url;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "CannotSaveChanges!");
                }

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}