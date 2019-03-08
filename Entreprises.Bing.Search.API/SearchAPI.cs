using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Entreprises.Bing.Search.API
{
    public class SearchAPI
    {
        const string key = "enter key here";

        const string uri = "https://api.cognitive.microsoft.com/bing/v7.0/localbusinesses/search";        

        struct Search
        {
            public String json;
            public Dictionary<String, String> relevantHeaders;
        }

        public async Task<string> QueryAPI(string textQuery)
        {
            Search result = BingLocalEntrepriseSearch(textQuery, key);
            return ToJson(result.json);

        }
        static Search BingLocalEntrepriseSearch(string textQuery, string key)
        {
            var uriQuery = uri + "?q=" + Uri.EscapeDataString(textQuery) + "&mkt=en-us";
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = key;
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponseAsync().Result;
            string streamReaderJson = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            var searchResult = new Search();
            searchResult.json = streamReaderJson;
            searchResult.relevantHeaders = new Dictionary<String, String>();

            // Extract Bing HTTP headers
            foreach (String header in httpWebResponse.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = httpWebResponse.Headers[header];
            }

            return searchResult;
        }
        static string ToJson(string json)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(json))
                return result;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char item in json)
            {
                switch (item)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(item);
                else
                {
                    switch (item)
                    {
                        case '{':
                        case '[':
                            sb.Append(item);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(item);
                            break;
                        case ',':
                            sb.Append(item);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(item);
                            sb.Append(' ');
                            break;
                        default:
                            if (item != ' ') sb.Append(item);
                            break;
                    }
                }
            }
            result= sb.ToString().Trim();

            return result;
        }

    }
}
