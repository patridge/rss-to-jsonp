using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        private readonly IHttpClientFactory clientFactory;
        public HomeController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        /// <summary>
        /// Cheesy way of avoiding namespaces.
        /// </summary>
        private static XElement getElement(IEnumerable<XElement> source, string name)
        {
            return source.FirstOrDefault(e => e.Name.LocalName == name);
        }
        private async Task<XDocument> GetFeedAsync(string url, string acceptType, CancellationToken cancellationToken)
        {
            var httpClient = clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", acceptType);
            request.Headers.Add("User-Agent", "rss-to-jsonp");
            XDocument retrievedXdocument = null;
            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        retrievedXdocument = await XDocument.LoadAsync(responseStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            return retrievedXdocument;
        }
        public async Task<JsonResult> Atom(string url, string callback)
        {
            if (url == null)
            {
                return Json(null);
            }

            IEnumerable<XElement> rootElements = (await GetFeedAsync(url, "application/atom+xml", CancellationToken.None)).Root.Elements();
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
        public async Task<JsonResult> Rss(string url, string callback)
        {
            if (url == null)
            {
                return Json(null);
            }

            IEnumerable<XElement> rootElements = getElement((await GetFeedAsync(url, "application/rss+xml", CancellationToken.None)).Root.Elements(), "channel").Elements();
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
