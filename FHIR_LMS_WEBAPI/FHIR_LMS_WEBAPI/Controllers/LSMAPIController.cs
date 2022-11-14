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

        private JObject CheckGroupQuantity(JObject appointment, string token)
        {
            dynamic errmsg = new JObject();
            HTTPrequest HTTPrequest = new HTTPrequest();
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");

            //GET Slot -> Course Code
            string slotID = appointment["slot"][0]["reference"].ToString();
            JObject slot = HTTPrequest.getResource(fhirUrl, slotID, "", token);

            if (slot != null && slot["resourceType"] != null && (string)slot["resourceType"] == "Slot")
            {
                string courseCode = slot["specialty"][0]["coding"][0]["code"].ToString();

                //GET Group -> identifier=coursecode
                JObject group = HTTPrequest.getResource(fhirUrl, "Group", "?identifier=" + courseCode, token);

                if (group != null && (int)group["total"] == 1)
                {
                    //GET maximum number
                    int groupQty = group["entry"][0]["resource"]["quantity"] != null ? (int)group["entry"][0]["resource"]["quantity"] : 0;

                    //GET Appointment -> slot id
                    //GET "Booked" Appointment Quantity
                    string param = "?slot=" + slotID.Split('/')[1] + "&status=booked";
                    JObject appSearch = HTTPrequest.getResource(fhirUrl, "Appointment", param, token);

                    if (appSearch != null && (int)appSearch["total"] != 0)
                    {
                        int bookedQty = (int)appSearch["total"];

                        if (bookedQty < groupQty)
                        {
                            int diff = groupQty - bookedQty;

                            //GET "Waitlist" Appointments, Sort updated
                            param = "?slot=" + slotID.Split('/')[1] + "&status=waitlist";
                            appSearch = HTTPrequest.getResource(fhirUrl, "Appointment", param, token);
                            if (appSearch != null && (int)appSearch["total"] != 0)
                            {
                                //Check whether this Appointment 
                                int waitlistQty = (int)appSearch["total"];
                                diff = diff < waitlistQty ? diff : waitlistQty;
                                string appointmentID = appointment["id"].ToString();

                                for (int i = 0; i < diff; i++)
                                {
                                    string curID = appSearch["entry"][i]["resource"]["id"].ToString();
                                    if (curID == appointmentID)
                                    {
                                        // Update 
                                        JObject new_appointment = (JObject)appSearch["entry"][i]["resource"];
                                        new_appointment.Property("meta").Remove();
                                        new_appointment["status"] = "booked";
                                        JObject result = HTTPrequest.putResource(fhirUrl, "Appointment/" + appointmentID, new_appointment, token);
                                        return result;
                                    }
                                }

                                /**************** BATCH UPDATE ****************
                                //Create Transaction
                                JObject trans = JObject.Parse(File.ReadAllText(@"D:\110325102\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\JSON\transaction.json"));
                                JArray entry = (JArray)trans["entry"];

                                for (int i = 0; i < diff; i++)
                                {
                                    JObject entryObj = new JObject();
                                    entryObj["resource"] = appSearch["entry"][i]["resource"];       //Copy from search return

                                    //Update [status] -> booked
                                    entryObj["resource"]["status"] = "booked";

                                    //Remove [meta]
                                    ((JObject)entryObj["resource"]).Property("meta").Remove();

                                    //Get [resourceType] & [id]
                                    string resourceType = entryObj["resource"]["resourceType"].ToString();
                                    string appointmentID = entryObj["resource"]["id"].ToString();

                                    entryObj["request"] = new JObject
                                    {
                                        {"method", "PUT"},
                                        {"url", fhirUrl + resourceType +"/" + appointmentID }
                                    };

                                    entry.Add(entryObj);
                                ****************/


                            }
                        }
                    }
                    else
                    {
                        //Alert maximum course
                        errmsg.message = "This course has reached its maximum capacity. " +
                            "We have added your name into the waiting list." +
                            "You'll be able to see the course material once approved by admin.";
                        return errmsg;
                    }
                }
            }

            errmsg.message = "Update Appointment error";
            return errmsg;
        }


        // POST: API/
        [System.Web.Http.HttpPost]
        public IHttpActionResult SelectCourse([FromBody] userLogin user)
        {
            string token = string.Empty;
            var headers = Request.Headers;
            if (headers.Contains("Custom"))
            {
                token = headers.GetValues("Custom").First();
            }

            dynamic errmsg = new JObject();

            HTTPrequest HTTPrequest = new HTTPrequest();
            JObject loginData = JObject.Parse(File.ReadAllText(@"D:\110325102\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\JSON\UserLogin.json"));
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            string param = "?identifier=" + user.Email;
            object person = HTTPrequest.getResource(fhirUrl, "Person", param, token);

            if (person != null || (int)((JObject)person)["total"] != 0)
            {
                JObject personUser = (JObject)person;
                if ((int)personUser["total"] == 1)
                {
                    loginData["person"]["id"] = ((JToken)personUser["entry"][0]["resource"]["id"] != null) ? personUser["entry"][0]["resource"]["id"] : "";
                    loginData["person"]["name"] = ((JToken)personUser["entry"][0]["resource"]["name"][0]["text"] != null) ? personUser["entry"][0]["resource"]["name"][0]["text"] : "";
                    loginData["person"]["identifier"] = ((JToken)personUser["entry"][0]["resource"]["identifier"][0] != null) ? personUser["entry"][0]["resource"]["identifier"][0]["value"] : "";

                    if (personUser["entry"][0]["resource"]["link"] != null)
                    {
                        JObject role = (JObject)personUser["entry"][0]["resource"]["link"][0];

                        string roleID = role["target"]["reference"].ToString();
                        object userRole = null;
                        if (roleID.Split('/')[0] == "Practitioner")
                        {
                            userRole = HTTPrequest.getResource(fhirUrl, "PractitionerRole", "?practitioner=" + roleID.Split('/')[1], token);
                        }
                        else if (roleID.Split('/')[0] == "Patient")
                        {
                            loginData["patient"]["id"] = roleID.Split('/')[1] != null ? roleID.Split('/')[1] : "";
                            userRole = HTTPrequest.getResource(fhirUrl, roleID.Split('/')[0], '/' + roleID.Split('/')[1], token);

                            JObject patientUser = (JObject)userRole;
                            if (patientUser != null && patientUser["resourceType"] != null && (string)patientUser["resourceType"] == "Patient")
                            {
                                //Create Appointment Waitlist
                                JObject appointment = JObject.Parse(File.ReadAllText(@"D:\110325102\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\FHIR_LMS_WEBAPI\JSON\appointment.json"));
                                appointment["participant"][0]["actor"]["reference"] = "Patient/" + patientUser["id"];
                                appointment["participant"][0]["actor"]["display"] = patientUser["name"][0]["text"];

                                JObject new_appointment = HTTPrequest.postResource(fhirUrl, "Appointment", appointment, token);

                                //Check Group Quantity
                                JObject result = CheckGroupQuantity(new_appointment, token);//, result);
                                return Ok(result);

                            }
                        }
                    }
                }
            }
            errmsg.message = "Something went wrong.";
            errmsg.token = token;
            return Ok(errmsg);
        }

    }
}
