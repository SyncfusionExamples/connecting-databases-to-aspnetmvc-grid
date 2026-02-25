using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MyWebService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable Attribute Routing
            config.MapHttpAttributeRoutes();

            // Remove XML formatter (ensure JSON responses)
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.Remove(config.Formatters.XmlFormatter);


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
