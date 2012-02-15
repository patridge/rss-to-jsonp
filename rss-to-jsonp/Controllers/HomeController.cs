namespace rss_to_jsonp.Controllers {
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using System.Xml.Linq;
    using rss_to_jsonp.Helpers;
    using System;
    using System.Collections.Generic;
    public class HomeController : Controller {
        /// <summary>
        /// Cheesy way of avoiding namespaces.
        /// </summary>
        private static XElement getElement(IEnumerable<XElement> source, string name) {
            return source.FirstOrDefault(e => e.Name.LocalName == name);
        }
        private static XDocument GetFeed(string url) {
            return XDocument.Load(url);
        }
        public JsonResult Atom(string url, string callback) {
            if (url == null) {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            IEnumerable<XElement> rootElements = GetFeed(url).Root.Elements();
            IEnumerable<XElement> entryElements = rootElements.Where(e => e.Name.LocalName == "entry");
            var entries = entryElements.Select(e => {
                IEnumerable<XElement> entryDetails = e.Elements();
                return new {
                    id = (string)getElement(entryDetails, "id"),
                    title = (string)getElement(entryDetails, "title"),
                    link = (string)(getElement(entryDetails, "link").Attributes().FirstOrDefault(a => a.Name.LocalName == "href")),
                    published = (DateTime)getElement(entryDetails, "published"),
                    updated = (DateTime)getElement(entryDetails, "updated"),
                    content = (string)getElement(entryDetails, "content")
                };
            });
            JsonpResult result = new JsonpResult(new {
                title = (string)getElement(rootElements, "title"),
                updated = (DateTime)getElement(rootElements, "updated"),
                entries = entries
            }, callback);
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }
        public JsonResult Rss(string url, string callback) {
            if (url == null) {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            IEnumerable<XElement> rootElements = getElement(GetFeed(url).Root.Elements(), "channel").Elements();
            IEnumerable<XElement> entryElements = rootElements.Where(e => e.Name.LocalName == "item");
            var entries = entryElements.Select(e => {
                IEnumerable<XElement> entryDetails = e.Elements();
                return new {
                    guid = (string)getElement(entryDetails, "guid"),
                    title = (string)getElement(entryDetails, "title"),
                    link = (string)(getElement(entryDetails, "link").Attributes().FirstOrDefault(a => a.Name.LocalName == "href")),
                    pubDate = (DateTime)getElement(entryDetails, "pubDate"),
                    description = (string)getElement(entryDetails, "description")
                };
            });
            JsonpResult result = new JsonpResult(new {
                title = (string)getElement(rootElements, "title"),
                description = (string)getElement(rootElements, "description"),
                lastBuildDate = (DateTime)getElement(rootElements, "lastBuildDate"),
                entries = entries
            }, callback);
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }
    }
}
