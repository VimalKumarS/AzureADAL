using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using System.Threading.Tasks;
using Microsoft.Owin;

[assembly: OwinStartup("BrowsertoWebApp" ,typeof(BrowsertoWebApp.App_Start.Startup))]
namespace BrowsertoWebApp.App_Start
{
    public class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string addInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tennant"];
        private static string postLogoutredirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirect"];

        string authority = string.Format(CultureInfo.InvariantCulture, addInstance, tenant);
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority=authority,
                PostLogoutRedirectUri=postLogoutredirectUri,
                Notifications= new OpenIdConnectAuthenticationNotifications
                {
                    //AuthorizationCodeReceived = context =>
                    //{
                    //    var code = context.Code;
                    //    ClientCredential credentail = new ClientCredential(clientId, Appkey);
                         
                    //},

                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/error/message=" + context.Exception.Message);
                        return Task.FromResult(0);
                    }
                }

            });
        }
    }
}