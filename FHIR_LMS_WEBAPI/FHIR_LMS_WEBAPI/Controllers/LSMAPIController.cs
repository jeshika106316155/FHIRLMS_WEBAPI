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
using System.Collections.Specialized;

namespace FHIR_LMS_WEBAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LSMAPIController : ApiController
    {

        [System.Web.Http.HttpPost]
        public IHttpActionResult SelectCourse([FromBody] userLogin user)
        {
            SelectCourse selectCourse = new SelectCourse();
            string token = string.Empty;
            var headers = Request.Headers;
            if (headers.Contains("Authorization"))
            {
                token = headers.GetValues("Authorization").First();
            }
            else
            {
                return Unauthorized();
            }

            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            HTTPrequest HTTPrequest = new HTTPrequest();
            
            JObject loginData = JObject.Parse(File.ReadAllText(ConfigurationManager.AppSettings.Get("LoginJSON")));

            //Get Person ID
            string[] arg = user.personId.Split('/');
            if(arg[0]=="Person" && arg.Length >= 2)
            {
                loginData["person"]["id"] = arg[1];
            }
            else
            {
                return BadRequest("PersonID not found.");
            }

            //Get Patient ID
            arg = user.patientId.Split('/');
            if (arg[0] == "Patient" && arg.Length >= 2)
            {
                loginData["patient"]["id"] = arg[1];
            }
            else
            {
                return BadRequest("PatientID not found.");
            }

            loginData["schedule"]["id"] = "860";

            //Check Login Data (Person == Patient)
            loginData["errmsg"] = "Check Login Person failed.";
            string param = '/' + loginData["person"]["id"].ToString();
            JObject result = HTTPrequest.getResource(fhirUrl, "Person", param, token, selectCourse.GetUserRole, loginData);
            
            return Ok(result);
        }


    }
}
