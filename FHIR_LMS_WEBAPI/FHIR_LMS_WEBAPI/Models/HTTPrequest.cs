using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace FHIR_LMS_WEBAPI.Models
{
    public class HTTPrequest
    {
        public object getResource(string fhirUrl, string ResourceName, string Parameter, string FHIRResponseType) //, Action <object> CallbackFunc
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl + ResourceName + Parameter);
                requestHttp.ContentType = "application/json";
                requestHttp.Method = "GET";
                var response = (HttpWebResponse)requestHttp.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        JObject resultJson = JObject.Parse(result);

                        //var id = resultJson["id"];
                        //if (CallbackFunc != null)
                        //{
                        //    CallbackFunc(resultJson);
                        //}
                        return resultJson;
                    }
                }
                else
                {
                    //reqMessage = "Error upload to FHIR Server!"; 
                    return JObject.Parse("{'total':0;'message':'Error upload to FHIR Server!'}");
                }
            }
            catch (Exception e)
            {
                return JObject.Parse("{'total':0;'message':'Error request to FHIR Server!'}");
            }
            return JObject.Parse("{'total':0;'message':'Error request to FHIR Server!'}");
        }
    }
}