using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Threading.Tasks;

namespace BrowsertoWebApp.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = "/"
                    }, OpenIdConnectAuthenticationDefaults.AuthenticationType
                    );
            }
        }
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType,CookieAuthenticationDefaults.AuthenticationType
                );
        }
    }
}