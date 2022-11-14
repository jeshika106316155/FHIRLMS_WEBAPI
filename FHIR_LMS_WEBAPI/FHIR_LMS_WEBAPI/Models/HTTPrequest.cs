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
        public JObject getResource(string fhirUrl, string ResourceName, string Parameter, string FHIRResponseType) //, Action <object> CallbackFunc
        {
            dynamic errmsg = new JObject();

            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl + ResourceName + Parameter);
                requestHttp.ContentType = "application/json";
                requestHttp.Method = "GET";
                requestHttp.Headers["Authorization"] = "Bearer " + "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ6QDEiLCJzY29wZSI6W10sImV4cCI6MTY2NTU4Mjc1MjAwMCwiaWF0IjoxNjY1NDYyNzUyMDAwfQ.gPIrRMNwn01fYXgyfOKoDAmFvrZx9v1CNNgh-iqB8-k99m6PoBOg0AOpspW_O9BWVL35AkU8pqCXptDGkXouEFM02dnmr26AFoSULKUr0YBVjDB_kxcWB1RQWp-9dPqC8uP1Abtt9Wq6u5Zx3K5wL2eXq_zFoBwZDCKkbv-YDtB9LfaJpF-GH_WDx2xxQm-Pbv8WiaQt4MgpAR5ooU7oCu_L1XKprh2CroLNTpPOyromOqIBgKOgfQt-SufZ2ZzI0uIMLaCVfBbYy-0WTU0_lZr-FDk-9O53cIIFLY7JoDk-K5Nui0AUbQm4NNWOeDbtc6B9FxAbKVokuO4j8zyjpw";
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
                    errmsg.total = 0;
                    errmsg.message = "Error upload to FHIR Server!";
                    return errmsg;
                }
            }
            catch (Exception e)
            {
                errmsg.total = 0;
                errmsg.message = "Error request to FHIR Server!";
                return errmsg;
            }
            errmsg.total = 0;
            errmsg.message = "Error request to FHIR Server!";
            return errmsg;
        }

        public JObject postResource(string fhirUrl, string ResourceName, JObject body, string FHIRResponseType) //, Action <object> CallbackFunc
        {
            dynamic errmsg = new JObject();

            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl + ResourceName);
                requestHttp.ContentType = "application/json";
                requestHttp.Method = "POST";
                requestHttp.Headers["Authorization"] = "Bearer " + "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ6QDEiLCJzY29wZSI6W10sImV4cCI6MTY2NTU4Mjc1MjAwMCwiaWF0IjoxNjY1NDYyNzUyMDAwfQ.gPIrRMNwn01fYXgyfOKoDAmFvrZx9v1CNNgh-iqB8-k99m6PoBOg0AOpspW_O9BWVL35AkU8pqCXptDGkXouEFM02dnmr26AFoSULKUr0YBVjDB_kxcWB1RQWp-9dPqC8uP1Abtt9Wq6u5Zx3K5wL2eXq_zFoBwZDCKkbv-YDtB9LfaJpF-GH_WDx2xxQm-Pbv8WiaQt4MgpAR5ooU7oCu_L1XKprh2CroLNTpPOyromOqIBgKOgfQt-SufZ2ZzI0uIMLaCVfBbYy-0WTU0_lZr-FDk-9O53cIIFLY7JoDk-K5Nui0AUbQm4NNWOeDbtc6B9FxAbKVokuO4j8zyjpw";
                string postBody = body.ToString();
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postBody);
                using (Stream reqStream = requestHttp.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                var response = (HttpWebResponse)requestHttp.GetResponse();
                if (response.StatusCode == HttpStatusCode.Created)
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
                    errmsg.total = 0;
                    errmsg.message = "Error upload to FHIR Server!";
                    return errmsg;
                }
            }
            catch (Exception e)
            {
                errmsg.total = 0;
                errmsg.message = "Error request to FHIR Server!";
                return errmsg;
            }
            errmsg.total = 0;
            errmsg.message = "Error request to FHIR Server!";
            return errmsg;
        }
    }
}