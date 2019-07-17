using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;
using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using System.Xml;

namespace GetMyFeeEstimateForAmazonMWS.Controllers
{
    public class HomeController : Controller
    {
        public MarketplaceWebServiceProductsConfig _productConfig;
        public MarketplaceWebServiceProducts.MarketplaceWebServiceProducts _productClient;
        public static string sellerId = "";
        public static string marketplaceID = "";
        public static string accessKeyID = "";
        public static string secretKey = "";
        public static string serviceURL = "";
        public HomeController()
        {
            sellerId = ConfigurationManager.AppSettings["SellerId"];
            marketplaceID = ConfigurationManager.AppSettings["MarketplaceID"];
            accessKeyID = ConfigurationManager.AppSettings["AccessKeyID"];
            secretKey = ConfigurationManager.AppSettings["SecretKey"];
            serviceURL = ConfigurationManager.AppSettings["ServiceURL"];
            _productConfig = new MarketplaceWebServiceProductsConfig { ServiceURL = serviceURL };
            _productClient = new MarketplaceWebServiceProductsClient("MWS", "1.0", accessKeyID, secretKey, _productConfig);
        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetFeeEstimateData(string ASIN)
        {
            try
            {
                StreamWriter BaseTag;
                var uploadRootFolder = AppDomain.CurrentDomain.BaseDirectory + "\\Reports";
                Directory.CreateDirectory(uploadRootFolder);
                var directoryFullPath = uploadRootFolder;
                string targetFile = Path.Combine(directoryFullPath, "FeeEstimateResponse_" + Guid.NewGuid() + ".xml");
                var response = InvokeGetMyFeesEstimate(ASIN);
                BaseTag = System.IO.File.CreateText(targetFile);
                BaseTag.Write(response.ToXML());
                BaseTag.Close();
                XmlDocument root = new XmlDocument();
                root.Load(targetFile);
                XmlNodeList elemList1 = root.GetElementsByTagName("FeeAmount");
                var data = elemList1[3].InnerText;
                var currency = data.Substring(0, 3);
                var fee = data.Substring(3);
                return Json(new { result = true, currency = currency, fee = fee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public GetMyFeesEstimateResponse InvokeGetMyFeesEstimate(string ASIN)
        {
            try
            {
                GetMyFeesEstimateRequest request = new GetMyFeesEstimateRequest();
                request.SellerId = sellerId;
                request.MWSAuthToken = "example";
                FeesEstimateRequestList feesEstimateRequestList = new FeesEstimateRequestList();
                feesEstimateRequestList.FeesEstimateRequest.Add(new FeesEstimateRequest
                {
                    MarketplaceId = marketplaceID,
                    IdType = "ASIN",
                    IdValue = ASIN,
                    PriceToEstimateFees = new PriceToEstimateFees { ListingPrice = new MoneyType { Amount = 0M, CurrencyCode = "USD" } },
                    Identifier = "request_" + Guid.NewGuid().ToString(),
                    IsAmazonFulfilled = true
                });
                request.FeesEstimateRequestList = feesEstimateRequestList;
                return _productClient.GetMyFeesEstimate(request);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}