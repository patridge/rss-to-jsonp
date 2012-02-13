namespace rss_to_jsonp {
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using rss_to_jsonp.Helpers;

    public class MvcApplication : System.Web.HttpApplication {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRouteSeo(
                "HomeControllerOmit",
                "{action}",
                new { controller = "Home", action = "Index" }
            );
        }

        protected void Application_PreSendRequestHeaders() {
            HttpContext.Current.Response.Headers.Remove("Server");
        }
        protected void Application_Start() {
            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}