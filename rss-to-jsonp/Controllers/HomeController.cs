using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using rss_to_jsonp.Helpers;
using rss_to_jsonp.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;

namespace rss_to_jsonp.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Cheesy way of avoiding namespaces.
        /// </summary>
        private static XElement getElement(IEnumerable<XElement> source, string name)
        {
            return source.FirstOrDefault(e => e.Name.LocalName == name);
        }
        private static XDocument GetFeed(string url)
        {
            return XDocument.Load(url);
        }
        public JsonResult Atom(string url, string callback)
        {
            if (url == null)
            {
                return Json(null);
            }

            IEnumerable<XElement> rootElements = GetFeed(url).Root.Elements();
            IEnumerable<XElement> entryElements = rootElements.Where(e => e.Name.LocalName == "entry");
            var entries = entryElements.Select(e => {
                IEnumerable<XElement> entryDetails = e.Elements();
                return new
                {
                    id = (string)getElement(entryDetails, "id"),
                    title = (string)getElement(entryDetails, "title"),
                    link = (string)(getElement(entryDetails, "link").Attributes().FirstOrDefault(a => a.Name.LocalName == "href")),
                    published = (DateTime)getElement(entryDetails, "published"),
                    updated = (DateTime)getElement(entryDetails, "updated"),
                    content = (string)getElement(entryDetails, "content"),
                };
            });
            JsonpResult result = new JsonpResult(new
            {
                title = (string)getElement(rootElements, "title"),
                updated = (DateTime)getElement(rootElements, "updated"),
                entries = entries,
            }, callback);
            return result;
        }
        public JsonResult Rss(string url, string callback)
        {
            if (url == null)
            {
                return Json(null);
            }

            IEnumerable<XElement> rootElements = getElement(GetFeed(url).Root.Elements(), "channel").Elements();
            IEnumerable<XElement> entryElements = rootElements.Where(e => e.Name.LocalName == "item");
            var entries = entryElements.Select(e => {
                IEnumerable<XElement> entryDetails = e.Elements();
                return new
                {
                    guid = (string)getElement(entryDetails, "guid"),
                    title = (string)getElement(entryDetails, "title"),
                    link = (string)(getElement(entryDetails, "link").Attributes().FirstOrDefault(a => a.Name.LocalName == "href")),
                    pubDate = (DateTime)getElement(entryDetails, "pubDate"),
                    description = (string)getElement(entryDetails, "description")
                };
            });
            JsonpResult result = new JsonpResult(new
            {
                title = (string)getElement(rootElements, "title"),
                description = (string)getElement(rootElements, "description"),
                lastBuildDate = (DateTime)getElement(rootElements, "lastBuildDate"),
                entries = entries
            }, callback);
            return result;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
