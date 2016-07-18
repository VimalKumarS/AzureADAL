using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WebAPiAD.Controllers
{
    [System.Web.Http.Authorize]
    public class LocationController : ApiController
    {
        private static string trustedcallerId = ConfigurationManager.AppSettings["ida:trustedCallerClientId"];
        public Models.Location Get(string cityname)
        {
            string currentCallerClientID = ClaimsPrincipal.Current.FindFirst("appid").Value;
            if(currentCallerClientID == trustedcallerId) { }
            return new Models.Location() { Latitude = 10, Longitude = 20 };
        }
    }
}