using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.ElasticScale;
using mpbdmService.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace mpbdmService.Controllers
{
    // TODO : NEEDS A CLEAN UP
    [RoutePrefix("api/Paypal")]
    public class PaypalTransactionController : ApiController
    {
        private string PAYPAL_SECRET = "EF4ddBAMccUN_eYfQ_N9bazr5eLBQl6LgUsMyL40zaxhI9M5TVnZSpmL4-jH";
        private string PAYPAL_CLIENTID = "Ab6CbxBfSarTyo7JXi1X8N_4RxbaTGJNl8l4hCj6GO4Mt6yGnSChonfPGsDd";

        [Route("buycoins")]
        [AuthorizeLevel(AuthorizationLevel.User)] 
        public async Task<HttpResponseMessage> BuyCoins(double amount , string redirectUrl = "")
        {           
            
            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            
            //////////////////////////////////////////////////////
            // Create Transactions Stuff
            /////////////////////////////////////////////////////
            Transactions trans = new Transactions();
            trans.Id = Guid.NewGuid().ToString();
            trans.Sender = "Paypal";
            trans.Reciever = shardKey;
            trans.SenderMoney = 999999;
            trans.Status = "Pending";
            trans.RecieverMoney = getRecieverMoney(shardKey,db);
            trans.Amount = amount.ToString();
            db.Transactions.Add(trans);

            try
            {
                db.SaveChanges();   
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Saving to database!");
            }
            //////////////////////////////////////////////////////
            // PayPal Stuff
            /////////////////////////////////////////////////////
            Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
            sdkConfig.Add("mode", "sandbox");
            string accessToken = new OAuthTokenCredential(PAYPAL_CLIENTID, PAYPAL_SECRET, sdkConfig).GetAccessToken();
            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = sdkConfig;

            Amount amnt = new Amount();
            amnt.currency = "EUR";
            amnt.total = amount.ToString();
            
            List<Transaction> transactionList = new List<Transaction>();
            Transaction tran = new Transaction();
            tran.description = "JustPhoneBook";
            tran.item_list = new ItemList
            {
                items = new List<Item>{
                        new Item{
                        name = "Coins for JustPhoneBook Wallet",
                        currency = "EUR",
                        price = amount.ToString(),
                        quantity = "1",
                        description = "Buy coins!"
                        }
                }
            };
            tran.amount = amnt;
            transactionList.Add(tran);

            Payer payr = new Payer();
            payr.payment_method = "paypal";

            RedirectUrls redirUrls = new RedirectUrls();
            redirUrls.cancel_url = "http://elasticscale.azure-mobile.net/api/Paypal/confirm?status=cancel&transactionId=" + trans.Id + "&shardKey=" + shardKey;
            redirUrls.return_url = "http://elasticscale.azure-mobile.net/api/Paypal/confirm?status=success&transactionId=" + trans.Id + "&shardKey=" + shardKey;

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactionList;
            pymnt.redirect_urls = redirUrls;

            Payment createdPayment = pymnt.Create(apiContext);
            trans.AccessToken = accessToken;
            trans.Token = createdPayment.token;
            trans.PaymentId = createdPayment.id;
            trans.RedirectUrl = redirectUrl;
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Saving to database 2!");
            }

            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            // TODO CHECK TO CREATE A 302 REDIRECT
            var b = Request.CreateResponse(HttpStatusCode.OK, createdPayment.links.Where(s => s.rel == "approval_url").FirstOrDefault().href);
            return b;
        }
        [Route("confirm")]
        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [HttpGet]
        public async Task<HttpResponseMessage> PaymentConfirm(string status, string transactionId, string shardKey, string token, string paymentId = "", string PayerID = "")
        {
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            Transactions trans = db.Transactions.Find(transactionId);
            if (trans == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest , "The transaction doesnt exist!");
            }
            if (trans.Status != "Pending")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "The transaction is NOT PENDING!");
            }
            if (status == "cancel")
            {
                trans.Status = "Cancelled";
            }
            else if (status == "success")
            {
                trans.RecieverMoney += double.Parse(trans.Amount);
                trans.SenderMoney -= double.Parse(trans.Amount);

                trans.Status = "Completed";
                
                Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
                sdkConfig.Add("mode", "sandbox");
                string accessToken = trans.AccessToken;
                APIContext apiContext = new APIContext(accessToken);
                apiContext.Config = sdkConfig;

                Payment pymnt = Payment.Get(apiContext , paymentId);
                PaymentExecution pymntExecution = new PaymentExecution();
                pymntExecution.payer_id = (PayerID);
                Payment executedPayment = pymnt.Execute(apiContext, pymntExecution);
                if (executedPayment.state != "approved")
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Payment wasnt approved!");
                }
            }

            db.SaveChanges();
            if (Uri.IsWellFormedUriString(trans.RedirectUrl , UriKind.Absolute))
            {
                var resp = Request.CreateResponse(HttpStatusCode.Redirect);
                resp.Headers.Location = new Uri(trans.RedirectUrl);
                return resp;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
        // TODO LOOK AT THIS \/
        public static double getRecieverMoney(string shardKey ,mpbdmContext<Guid> db )
        {
            var res = db.Transactions.Where(s => s.Status == "Completed" && (s.Reciever == shardKey || s.Sender == shardKey)).OrderByDescending(s => s.UpdatedAt).FirstOrDefault();
            if (res != null)
            {
                if (res.Reciever == shardKey) return res.RecieverMoney;
                else return res.SenderMoney;
            }
            return 0.0;
        }







        /// <summary>
        ///  Subscribe Creation 
        /// </summary>
        /// <param name="redirectUrl"> The URL to redirect the user after subscription is completed! </param>
        /// <returns></returns>
        [Route("subscribe")]
        [AuthorizeLevel(AuthorizationLevel.User)]
        public async Task<HttpResponseMessage> Subscribe(string redirectUrl = "")
        {
            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);

            //////////////////////////////////////////////////////
            // Create Subscription Stuff
            /////////////////////////////////////////////////////
            Subscriptions trans = new Subscriptions();
            trans.Id = Guid.NewGuid().ToString();
            trans.CompaniesID = shardKey;
            trans.Status = "Pending";
            trans.Type = SUBSCRIPTION_TYPE.PREMIUM;
            trans.RedirectUrl = redirectUrl;
            db.Subscriptions.Add(trans);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Saving to database! Inner : "  + ex.InnerException);
            }
            //////////////////////////////////////////////////////
            // PayPal Stuff
            /////////////////////////////////////////////////////
            Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
            sdkConfig.Add("mode", "sandbox");
            string accessToken = new OAuthTokenCredential(PAYPAL_CLIENTID, PAYPAL_SECRET, sdkConfig).GetAccessToken();
            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = sdkConfig;
            //////////////////////////
            // PLAN CREATION
            ////////////////////////
            var plan = createPremiumSubscriptionPlan(trans.Id , shardKey);
            Plan createdPlan;
            try{
                createdPlan = plan.Create(apiContext);
            }
            catch (PayPal.PaymentsException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError , ex);
            }
            //////////////////////////////////////////
            // Activate the plan
            var patchRequest = new PatchRequest()
            {
                new Patch()
                {
                    op = "replace",
                    path = "/",
                    value = new Plan() { state = "ACTIVE" }
                }
            };
            // UPDATE THE PLAN
            createdPlan.Update(apiContext, patchRequest);
            ///////////////////////////////////////////////////////////
            // CREATE AGGREMENT
            var payer = new Payer() { payment_method = "paypal" };
            var agreement = new Agreement()
            {
                name = "JustPhoneBook - Monthly Premium Subscription",
                description = "Agreement for monthly premium subscription",
                start_date = String.Format( "{0:s}Z" , DateTime.Now.AddDays(1)),
                payer = payer,
                plan = new Plan() { id = createdPlan.id }
            };
            
            Agreement createdAgreement;
            try
            {
                createdAgreement = agreement.Create(apiContext);
            }
            catch (PayPal.PaymentsException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError , agreement.start_date);
            }
            /////////////////////////////////////////////////////
            
            trans.Token = createdAgreement.token;
            trans.AccessToken = accessToken;
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error Saving to database 2! Inner : " + ex.InnerException);
            }

            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            // TODO CHECK TO CREATE A 302 REDIRECT
            var b = Request.CreateResponse(HttpStatusCode.OK, createdAgreement.links.Where(s => s.rel == "approval_url").FirstOrDefault().href);
            return b;
        }

        [Route("confirmSubscription")]
        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [HttpGet]
        public async Task<HttpResponseMessage> SubscriptionConfirm(string status, string subscriptionId, string shardKey, string token, string paymentId = "", string PayerID = "")
        {
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            Subscriptions trans = db.Subscriptions.Find(subscriptionId);
            if (trans == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "The transaction doesnt exist!");
            }
            if (trans.Status != "Pending")
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "The transaction is NOT PENDING!");
            }
            if (status == "cancel" || trans.Token != token )
            {
                trans.Status = "Cancelled";
            }
            else if (status == "success")
            {
                trans.StartingDate = DateTime.Now;
                trans.ExpirationDate = DateTime.Now.AddDays(30);
                trans.Status = "Completed";

                Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
                sdkConfig.Add("mode", "sandbox");
                string accessToken = trans.AccessToken;
                APIContext apiContext = new APIContext(accessToken);
                apiContext.Config = sdkConfig;

                var agreement = new Agreement() { token = token };
                var executedAgreement = agreement.Execute(apiContext);
                
            }

            db.SaveChanges();
            if (Uri.IsWellFormedUriString(trans.RedirectUrl, UriKind.Absolute))
            {
                var resp = Request.CreateResponse(HttpStatusCode.Redirect);
                resp.Headers.Location = new Uri(trans.RedirectUrl);
                return resp;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }




        //////////////////////////////////////////////////////////////////////////////////////
        //HELPER
        ////////////////////////////////////////////////////////////////////////////////////
        private static Currency GetCurrency(string value)
        {
            return new Currency() { value = value, currency = "EUR" };
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // STATIC PAYPAL SUBSCRIPTION PLANS
        ////////////////////////////////////////////////////////////////////////////////////
        public static Plan createPremiumSubscriptionPlan( string subId , string shardKey)
        {
            // ### Create the Billing Plan
            // Both the trial and standard plans will use the same shipping
            // charge for this example, so for simplicity we'll create a
            // single object to use with both payment definitions.
            //var shippingChargeModel = new ChargeModel()
            //{
            //    type = "SHIPPING",
            //    amount = GetCurrency("9.99")
            //};

            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
           return new Plan
            {
                name = "Monthly Premium Subscription to JustPhoneBook",
                description = "Monthly Premium subscription(description).",
                type = "INFINITE",
                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                merchant_preferences = new MerchantPreferences()
                {
                    return_url = "http://elasticscale.azure-mobile.net/api/Paypal/confirmSubscription?status=success&subscriptionId=" + subId + "&shardKey=" + shardKey,
                    cancel_url = "http://elasticscale.azure-mobile.net/api/Paypal/confirmSubscription?status=cancel&subscriptionId=" + subId  + "&shardKey=" + shardKey,
                    auto_bill_amount = "YES",
                    initial_fail_amount_action = "CONTINUE",
                    max_fail_attempts = "0"
                },
                payment_definitions = new List<PaymentDefinition>
                {
                    // Define a trial plan that will only charge $9.99 for the first
                    // month. After that, the standard plan will take over for the
                    // remaining 11 months of the year.
                    new PaymentDefinition()
                    {
                        name = "Mothly Premium Subscription",
                        type = "REGULAR",
                        frequency = "MONTH",
                        frequency_interval = "1",
                        amount = GetCurrency("7.99"),
                        cycles = "0"
                    }
                    // UNCOMMENT TO CREATE AN AGGREMENT WITH DIFFERENT CHARGES! 
                    //, 
                    // Define the standard payment plan. It will represent a monthly
                    // plan for $19.99 USD that charges once month for 11 months.
                    //new PaymentDefinition
                    //{
                    //    name = "Standard Plan",
                    //    type = "REGULAR",
                    //    frequency = "MONTH",
                    //    frequency_interval = "1",
                    //    amount = GetCurrency("19.99"),
                    //    // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                    //    cycles = "11",
                    //    charge_models = new List<ChargeModel>
                    //    {
                    //        new ChargeModel
                    //        {
                    //            type = "TAX",
                    //            amount = GetCurrency("2.47")
                    //        },
                    //        shippingChargeModel
                    //    }
                    //}
                }
            };
        }


















        /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////                                                    //////////////////////////
        ///////////////////               WEBHOOK                              //////////////////////////
        ///////////////////                                                    //////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////

        [Route("webhook")]
        [AuthorizeLevel(AuthorizationLevel.Anonymous)]
        [HttpPost]
        public async Task<HttpResponseMessage> Webhook()
        {
            
            return null;
        }

                                              
    }
}
