using System.Collections.Generic;
using System.Net.Http;

namespace httpapi.Models
{
    public class GetModel
    {
        public string url { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public string origin { get; set; } 
    }
    
    public class CompressedContentModel : GetModel
    {
        public bool gzipped { get; set; }
        public bool deflated { get; set; }
        public HttpMethod method { get; set; }

    }
}