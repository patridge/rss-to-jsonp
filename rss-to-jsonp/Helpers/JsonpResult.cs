namespace rss_to_jsonp.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;

    /// <summary>
    /// When given a callback, wraps a JsonResult in JSONP format.
    /// </summary>
    public class JsonpResult : JsonResult
    {
        public JsonpResult(object value, string callback = null) : base(value)
        {
            Callback = callback;
        }
        public string Callback { get; set; }
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (this.Callback == null)
            {
                // No callback, just let the default JsonResult system handle the request.
                await base.ExecuteResultAsync(context);
                return;
            }

            if (null == context)
            {
                throw new ArgumentNullException("controllerContext");
            }

            HttpResponse response = context.HttpContext.Response;
            if (!string.IsNullOrEmpty(base.ContentType))
            {
                // Allows ContentType to be set elsewhere, if needed.
                response.ContentType = base.ContentType;
            }
            else
            {
                // Callback = JSONP => not technically JSON (interpreted by browser by <script src> tag injection).
                response.ContentType = "application/javascript";
            }

            if (this.Value != null)
            {
                string json = JsonConvert.SerializeObject(this.Value);
                await response.WriteAsync(string.Format("{0}({1})", this.Callback, json));
            }
        }
    }
}
