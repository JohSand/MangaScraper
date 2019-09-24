//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Collections.Generic;
//using System.Linq;
//using AngleSharp.Html.Parser;


//namespace ConsoleApp2
//{

//    /// <summary>
//    /// A custom Http handler for Cloudflare protected servers
//    /// </summary>
//    public class CloudflareHttpHandler : HttpClientHandler
//    {
//        private static HtmlParser HtmlParser { get; } = new HtmlParser();

//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            var response = await base.SendAsync(request, cancellationToken);

//            if (CookieContainer.GetCookieHeader(request.RequestUri).Contains("cf_clearance"))
//                return response;

//            IEnumerable<string> values;

//            if (response.Headers.TryGetValues("refresh", out values) && values.FirstOrDefault().Contains("URL=/cdn-cgi/") && response.Headers.Server.ToString() == "cloudflare-nginx")
//            {
//                Console.WriteLine("Solving cloudflare challenge . . . ");

//                //string content = response.Content.ReadAsStringAsync().Result;
//                var htmlDocument = await HtmlParser.ParseDocumentAsync(await response.Content.ReadAsStreamAsync());
//                //var htmlDocument = new HtmlDocument();
//                //htmlDocument.LoadHtml(content);

//                var jschl_vc = htmlDocument.DocumentElement.QuerySelector(@".//input[@name=""jschl_vc""]").Attributes["value"].Value;
//                var pass = htmlDocument.DocumentElement.QuerySelector(@".//input[@name=""pass""]").Attributes["value"].Value;

//                var script = htmlDocument.DocumentElement.QuerySelector(@".//script").InnerHtml;

//                var regex = new string[3] {
//                @"setTimeout\(function\(\){(.+)},\s*\d*\s*\)\s*;",
//                @"^\n*\s*(var\s+.*?;)",
//                @"(?<=\s+;)(.+t.length;)"
//            };

//                string function, vars, calc;

//                function = Regex.Match(script, regex[0], RegexOptions.Singleline).Value;
//                vars = Regex.Match(function, regex[1], RegexOptions.Multiline).Value;
//                calc = Regex.Match(function, regex[2], RegexOptions.Singleline).Value
//                    .Replace("a.value", "var result")
//                    .Replace("t.length", request.RequestUri.Host.Length.ToString()); ;

//                object result;
//                //using (var engine = new V8ScriptEngine())
//                //    result = engine.Evaluate("function getAnswer() {" + vars + calc + "return result;" + "} getAnswer();");

//                Thread.Sleep(5000);

//                var requestUri = request.RequestUri;

//                request.RequestUri = new Uri(requestUri, string.Format("cdn-cgi/l/chk_jschl?jschl_vc={0}&pass={1}&jschl_answer={2}", jschl_vc, pass, result.ToString()));

//                base.SendAsync(request, cancellationToken).Wait();

//                request.RequestUri = requestUri;

//                return await base.SendAsync(request, cancellationToken);
//            }

//            return response;
//        }
//    }
//}