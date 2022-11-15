using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FHIR_LMS_WEBAPI.Models
{
    public class userLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ScheduleID { get; set; }
    }
}