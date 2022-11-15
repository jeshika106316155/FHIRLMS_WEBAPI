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
        [System.Web.Http.HttpPost]
        public IHttpActionResult SelectCourse([FromBody] userLogin user)
        {
            SelectCourse selectCourse = new SelectCourse();
            string token = string.Empty;
            var headers = Request.Headers;
            if (headers.Contains("Custom"))
            {
                token = headers.GetValues("Custom").First();
            }

            dynamic errmsg = new JObject();

            HTTPrequest HTTPrequest = new HTTPrequest();
            JObject loginData = JObject.Parse(File.ReadAllText(ConfigurationManager.AppSettings.Get("LoginJSON")));
            loginData["person"]["identifier"] = user.Email;
            loginData["schedule"]["id"] = user.ScheduleID;
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");

            loginData["errmsg"] = "GET Person failed.";
            string param = "?identifier=" + loginData["person"]["identifier"];
            JObject result = HTTPrequest.getResource(fhirUrl, "Person", param, token, selectCourse.GetUserRole, loginData);

            return Ok(result);
        }

        
    }
}
