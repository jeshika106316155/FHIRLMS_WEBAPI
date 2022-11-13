using FHIR_LMS_WEBAPI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web.Http.Cors;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FHIR_LMS_WEBAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LSMAPIController : ApiController
    {
        // POST: API/
        [System.Web.Http.HttpPost]
        public IHttpActionResult SelectCourse([FromBody] userLogin user)
        {
            HTTPrequest HTTPrequest = new HTTPrequest();
            JObject loginData = JObject.Parse(File.ReadAllText(@"D:\Jeshika\Research\LMS\LMS_API\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\JSON\UserLogiin.json"));
            string fhirUrl = ConfigurationManager.AppSettings.Get("PatientPortalFHIR_Url");
            string FHIRResponseType = ConfigurationManager.AppSettings.Get("FHIRResponseType");
            string param = "?identifier=" + user.Email;
            object person = HTTPrequest.getResource(fhirUrl, "Person", param, FHIRResponseType);

            if (person != null || (int)((JObject)person)["total"] != 0)
            {
                JObject personUser = (JObject)person;
                if ((int)personUser["total"] == 1)
                {
                    loginData["person"]["id"] = ((JToken)personUser["entry"][0]["resource"]["id"] != null) ? personUser["entry"][0]["resource"]["id"] : "";
                    loginData["person"]["name"] = ((JToken)personUser["entry"][0]["resource"]["name"][0]["text"] != null) ? personUser["entry"][0]["resource"]["name"][0]["text"] : "";
                    loginData["person"]["identifier"] = ((JToken)personUser["entry"][0]["resource"]["identifier"][0] != null) ? personUser["entry"][0]["resource"]["identifier"][0]["value"] : "";

                    if ((JToken)personUser["entry"][0]["resource"]["link"] != null)
                    {
                        foreach (JObject role in personUser["entry"][0]["resource"]["link"])
                        {
                            string roleID = role["target"]["reference"].ToString();
                            object userRole = null;
                            if (roleID.Split('/')[0] == "Practitioner")
                            {
                                userRole = HTTPrequest.getResource(fhirUrl, "PractitionerRole", "?practitioner=" + roleID.Split('/')[1], FHIRResponseType);
                            }
                            else if (roleID.Split('/')[0] == "Patient")
                            {
                                userRole = HTTPrequest.getResource(fhirUrl, roleID.Split('/')[0], '/' + roleID.Split('/')[1], FHIRResponseType);
                            }

                            JObject patientUser = (JObject)userRole;
                            if ((int)patientUser["total"] == 1)
                            {

                                //Create Appointment


                            }
                        }
                    }
                }
            }


            return Ok(new userLogin());
        }

    }
}
