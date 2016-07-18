using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.ActiveDirectory;
using System.Configuration;

[assembly: OwinStartup(typeof(WebAPiAD.App_Start.Startup))]

namespace WebAPiAD.App_Start
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);

            ConfigureAuth(app);

            app.Run(context =>
            {
                string t = DateTime.Now.Millisecond.ToString();
                return context.Response.WriteAsync(t + " Production OWIN App");
            });
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var azureADBearerAuthOption = new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            {
                Tenant = ConfigurationManager.AppSettings["ida:Tennant"]
            };
            azureADBearerAuthOption.TokenValidationParameters =
                new System.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                };

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(azureADBearerAuthOption);
        }
    }
}