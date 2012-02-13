namespace rss_to_jsonp.Helpers {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class SeoRoute : System.Web.Routing.Route {
        public SeoRoute(string url, IRouteHandler routeHandler) : base(url, routeHandler) { }
        public SeoRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler) : base(url, defaults, routeHandler) { }
        public SeoRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler) : base(url, defaults, constraints, routeHandler) { }
        public SeoRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) : base(url, defaults, constraints, dataTokens, routeHandler) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            var path = base.GetVirtualPath(requestContext, values);
            if (path != null) {
                var parts = path.VirtualPath.Split('?');
                if (parts.Count() == 2) {
                    var url = parts[0];
                    if (!url.EndsWith("/")) { url = parts[0] + "/"; }
                    // ToLowerInvariant path but not querystring.
                    path.VirtualPath = url.ToLowerInvariant() + "?" + parts[1];
                }
                else {
                    path.VirtualPath = path.VirtualPath.ToLowerInvariant();
                    if (string.Empty != path.VirtualPath && !path.VirtualPath.EndsWith("/")) {
                        path.VirtualPath = path.VirtualPath + "/";
                    }
                }
            }
            return path;
        }
    }

    public static class SeoRouteCollectionExtensions {
        public static void MapRouteSeo(this RouteCollection routes, string name, string url, object defaults) {
            routes.MapRouteSeo(name, url, defaults, null, null);
        }
        public static void MapRouteSeo(this RouteCollection routes, string name, string url, object defaults, object constraints) {
            routes.MapRouteSeo(name, url, defaults, constraints, null);
        }
        public static Route MapRouteSeo(this AreaRegistrationContext areaRegistrationContext, string name, string url, object defaults, object constraints, string[] namespaces) {
            if (areaRegistrationContext == null) { throw new ArgumentNullException("areaRegistrationContext"); }
            if (url == null) { throw new ArgumentNullException("url"); }

            if (namespaces == null && areaRegistrationContext.Namespaces != null) {
                namespaces = areaRegistrationContext.Namespaces.ToArray();
            }

            SeoRoute route = areaRegistrationContext.Routes.MapRouteSeo(name, url, defaults, constraints, namespaces);
            route.DataTokens["area"] = areaRegistrationContext.AreaName;

            // disabling the namespace lookup fallback mechanism keeps areas from accidentally picking up
            // controllers belonging to other areas
            bool useNamespaceFallback = namespaces == null || namespaces.Length == 0;
            route.DataTokens["UseNamespaceFallback"] = useNamespaceFallback;

            return route;
        }
        public static SeoRoute MapRouteSeo(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces) {
            if (routes == null) { throw new ArgumentNullException("routes"); }
            if (url == null) { throw new ArgumentNullException("url"); }
            var route = new SeoRoute(url, new MvcRouteHandler()) {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints)
            };
            route.DataTokens = route.DataTokens ?? new RouteValueDictionary();
            route.DataTokens.Add("RouteName", name);
            if ((namespaces != null) && (namespaces.Length > 0)) {
                route.DataTokens["Namespaces"] = namespaces;
            }
            if (String.IsNullOrEmpty(name)) { routes.Add(route); }
            else { routes.Add(name, route); }
            return route;
        }
    }
}