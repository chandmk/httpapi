using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace httpapi.web.Models
{
    public class SampleData
    {
        public SampleData()
        {
            form = new Dictionary<string, object>();
        }
        public string url { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public string origin { get; set; }

        public Dictionary<string, object> form { get; set; }

        public static SampleData WithDefaults()
        {
            var d = new SampleData { form = new Dictionary<string, object>() { { "k0", "v0" } } };
            return d;
        }

    }


    public class CompressedContentModel : SampleData
    {
        public bool gzipped { get; set; }
        public bool deflated { get; set; }
        public HttpMethod method { get; set; }

    }
}