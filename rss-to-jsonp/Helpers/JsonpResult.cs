namespace rss_to_jsonp.Helpers {
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    
    /// <summary>
    /// When given a callback, wraps a JsonResult in JSONP format.
    /// </summary>
    public class JsonpResult : JsonResult {
        public JsonpResult(object data, string callback = null) {
            Data = data;
            Callback = callback;
        }
        public string Callback { get; set; }
        public override void ExecuteResult(ControllerContext controllerContext) {
            if (this.Callback == null) {
                // No callback, just let the default JsonResult system handle the request.
                base.ExecuteResult(controllerContext);
                return;
            }

            if (null == controllerContext) {
                throw new ArgumentNullException("controllerContext");
            }

            HttpResponseBase response = controllerContext.HttpContext.Response;
            if (!string.IsNullOrEmpty(ContentType)) {
                // Allows ContentType to be set elsewhere, if needed.
                response.ContentType = ContentType;
            }
            else {
                // Callback = JSONP => not technically JSON (interpreted by browser by <script src> tag injection).
                response.ContentType = "application/javascript";
            }

            if (ContentEncoding != null) {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data != null) {
                var javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.RegisterConverters(new JavaScriptConverter[] { new ExpandoJsonConverter() });
                string json = javaScriptSerializer.Serialize(Data);
                response.Write(string.Format("{0}({1})", Callback, json));
            }
        }
    }
}
