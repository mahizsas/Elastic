﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace mpbdmService.Controllers
{
    public class UploadController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        public async Task<HttpResponseMessage> Post()
        {

            CloudStorageAccount acc = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Azure"].ConnectionString);
            CloudBlobClient blobClient = acc.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference("temp");

            await photoContainer.CreateIfNotExistsAsync();

            var provider = new AzureBlobMultipartFormDataStreamProvider(photoContainer);

            await this.Request.Content.ReadAsMultipartAsync(provider);

            //var photos = new List<PhotoViewModel>();

            foreach (var file in provider.FileData)
            {
                //the LocalFileName is going to be the absolute Uri of the blob (see GetStream)
                //use it to get the blob info to return to the client
                var blob = await photoContainer.GetBlobReferenceFromServerAsync(file.LocalFileName);
                await blob.FetchAttributesAsync();

                string url = blob.Uri.ToString();
                //provider.GetStream(this.RequestContext);
                //FileStream fs = new FileStream();
                //blob.DownloadToStream(fs);


                //FileStream fs = new FileStream(url, FileMode.Open, FileAccess.Read);
                HttpClient cl = new HttpClient();
                Stream ss = new MemoryStream();
                blob.DownloadToStream(ss);

                HSSFWorkbook templateWorkbook = new HSSFWorkbook(ss);

                HSSFSheet sheet = (HSSFSheet)templateWorkbook.GetSheet("Sheet1");


                azmobtestContext db = new azmobtestContext();
                for (int i = 1; true; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) break;

                    Contact cont = new Contact();
                    cont.FirstName = row.GetCell(0).RichStringCellValue.String;
                    cont.LastName = row.GetCell(1).RichStringCellValue.String;
                    cont.Email = row.GetCell(2).RichStringCellValue.String;
                    cont.Phone = row.GetCell(3).NumericCellValue.ToString();
                    cont.GroupID = row.GetCell(4).RichStringCellValue.String;
                    cont.Id = Guid.NewGuid().ToString();
                    cont.Deleted = false;
                    cont.Visible = true;

                    var chk = db.Set<Contact>().Where(s => s.Email == cont.Email && s.LastName == cont.LastName).FirstOrDefault();
                    if (chk != null)
                        continue;

                    db.Set<Contact>().Add(cont);

                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Propably the Foreign Key GroupId is wrong on some of your Contacts!!! Make sure the groupId exists!");
                }

            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}