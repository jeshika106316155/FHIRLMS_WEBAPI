﻿using System.Web;
using System.Web.Mvc;

namespace FHIR_LMS_WEBAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
