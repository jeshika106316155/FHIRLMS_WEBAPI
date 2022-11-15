using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace FHIR_LMS_WEBAPI.Models
{
    public class SelectCourse
    {
        public JObject GetUserRole(JObject personUser, JObject loginData, string token)
        {
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            HTTPrequest HTTPrequest = new HTTPrequest();

            JObject result = null;
            if ((int)personUser["total"] == 1)
            {
                loginData["person"]["id"] = personUser["entry"][0]["resource"]["id"] != null ? personUser["entry"][0]["resource"]["id"] : "";
                loginData["person"]["name"] = personUser["entry"][0]["resource"]["name"][0]["text"] != null ? personUser["entry"][0]["resource"]["name"][0]["text"] : "";
                loginData["person"]["identifier"] = personUser["entry"][0]["resource"]["identifier"][0] != null ? personUser["entry"][0]["resource"]["identifier"][0]["value"] : "";

                if (personUser["entry"][0]["resource"]["link"] != null)
                {
                    JObject role = (JObject)personUser["entry"][0]["resource"]["link"][0];

                    string roleID = role["target"]["reference"].ToString();

                    string param = string.Empty;

                    if (roleID.Split('/')[0] == "Practitioner")
                    {
                        param = "?practitioner=" + roleID.Split('/')[1];
                        result = HTTPrequest.getResource(fhirUrl, "PractitionerRole", param, token, GetSchedule, loginData);
                    }
                    else if (roleID.Split('/')[0] == "Patient")
                    {
                        loginData["patient"]["id"] = roleID.Split('/')[1];
                        loginData["errmsg"] = "GET Patient failed.";
                        param = '/' + loginData["patient"]["id"].ToString();
                        result = HTTPrequest.getResource(fhirUrl, "Patient", param, token, GetSchedule, loginData);
                    }
                }
            }
            return result;
        }

        public JObject GetSchedule(JObject patientUser, JObject loginData, string token)
        {
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            HTTPrequest HTTPrequest = new HTTPrequest();

            //GET Schedule -> get CourseID
            loginData["errmsg"] = "GET Schedule failed.";
            string param = '/' + loginData["schedule"]["id"].ToString();
            JObject result = HTTPrequest.getResource(fhirUrl, "Schedule", param, token, GetSlotID, loginData);
            return result;
        }

        public JObject GetSlotID(JObject schedule, JObject loginData, string token)
        {
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            HTTPrequest HTTPrequest = new HTTPrequest();

            loginData["schedule"]["courseCode"] = schedule["specialty"][0]["coding"][0]["code"].ToString();

            //GET SlotID
            loginData["errmsg"] = "GET Slot failed.";
            string param = "?schedule=" + loginData["schedule"]["id"];
            JObject result = HTTPrequest.getResource(fhirUrl, "Slot", param, token, CreateAppointment, loginData);
            return result;
        }

        public JObject CreateAppointment(JObject slot, JObject loginData, string token)
        {
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            HTTPrequest HTTPrequest = new HTTPrequest();

            loginData["slot"]["id"] = slot["entry"][0]["resource"]["id"];

            //Create Appointment Waitlist
            JObject appointment = JObject.Parse(File.ReadAllText(ConfigurationManager.AppSettings.Get("AppointmentJSON")));
            appointment["participant"][0]["actor"]["reference"] = "Patient/" + loginData["patient"]["id"];
            appointment["participant"][0]["actor"]["display"] = loginData["person"]["name"];
            appointment["slot"][0]["reference"] = "Slot/" + loginData["slot"]["id"];
            appointment["status"] = "waitlist";

            //POST new Appointment
            loginData["errmsg"] = "Create Appointment failed.";
            JObject result = HTTPrequest.postResource(fhirUrl, "Appointment", appointment, token, GetGroupQuantity, loginData);
            return result;
        }

        public JObject GetGroupQuantity(JObject appointment, JObject loginData, string token)
        {
            HTTPrequest HTTPrequest = new HTTPrequest();
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");

            loginData["appointment"]["id"] = appointment["id"];

            //GET Group -> identifier=coursecode
            loginData["errmsg"] = "GET Group quantity failed.";
            string param = "?identifier=" + loginData["schedule"]["courseCode"];
            JObject result = HTTPrequest.getResource(fhirUrl, "Group", param, token, GetBookedAppointment, loginData);

            return result;
        }
        public JObject GetBookedAppointment(JObject group, JObject loginData, string token)
        {
            HTTPrequest HTTPrequest = new HTTPrequest();
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");

            //GET maximum participant
            loginData["schedule"]["maxParticipant"] = group["entry"][0]["resource"]["quantity"] != null ? (int)group["entry"][0]["resource"]["quantity"] : 0;

            //GET Appointment -> slot id
            //GET "Booked" Appointment Quantity
            loginData["errmsg"] = "GET booked Appointment failed.";
            string param = "?slot=" + loginData["slot"]["id"] + "&status=booked";
            JObject result = HTTPrequest.getResource(fhirUrl, "Appointment", param, token, GetWaitlistAppointment, loginData);
            return result;

        }
        public JObject GetWaitlistAppointment(JObject appSearch, JObject loginData, string token)
        {
            JObject result = new JObject();
            HTTPrequest HTTPrequest = new HTTPrequest();
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");

            loginData["schedule"]["currentParticipant"] = appSearch["total"];

            int groupQty = (int)loginData["schedule"]["maxParticipant"];
            int bookedQty = (int)appSearch["total"];

            if (bookedQty < groupQty)
            {
                int diff = groupQty - bookedQty;

                //GET "Waitlist" Appointments, Sort updated
                string param = "?slot=" + loginData["slot"]["id"] + "&status=waitlist";
                result = HTTPrequest.getResource(fhirUrl, "Appointment", param, token, CheckCourseAvailability, loginData);
                return result;
            }
            //Alert maximum course
            result["message"] = "This course has reached its maximum capacity. " +
                "We have added your name into the waiting list." +
                "You'll be able to see the course material once approved by admin.";
            return result;
        }

        public JObject CheckCourseAvailability(JObject appSearch, JObject loginData, string token)
        {
            JObject result = new JObject();
            HTTPrequest HTTPrequest = new HTTPrequest();
            string fhirUrl = ConfigurationManager.AppSettings.Get("TZFHIR_Url");
            int diff = (int)loginData["schedule"]["maxParticipant"] - (int)loginData["schedule"]["currentParticipant"];
            int waitlistQty = (int)appSearch["total"];

            //diff = diff < waitlistQty ? diff : waitlistQty;

            string appointmentID = loginData["appointment"]["id"].ToString();

            var entry = JArray.Parse(appSearch["entry"].ToString());
            var requiredArticle = entry.First(a => a["resource"]["id"].ToString().Equals(appointmentID));
            int index = entry.IndexOf(requiredArticle);

            if (index != -1 && index < diff)
            {
                JObject new_appointment = (JObject)appSearch["entry"][index]["resource"];
                new_appointment.Property("meta").Remove();
                new_appointment["status"] = "booked";

                string param = '/' + appointmentID;
                result = HTTPrequest.putResource(fhirUrl, "Appointment" + param, new_appointment, token, null, loginData);
                return result;

            }
            //Alert maximum course
            result["message"] = "This course has reached its maximum capacity. " +
                "We have added your name into the waiting list." +
                "You'll be able to see the course material once approved by admin.";
            return result;
        }
    }
}