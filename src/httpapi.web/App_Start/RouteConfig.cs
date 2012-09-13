﻿using System.Web.Mvc;
using System.Web.Routing;

namespace httpapi.web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "Client/{action}/{id}",
                defaults: new { controller = "Client", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}