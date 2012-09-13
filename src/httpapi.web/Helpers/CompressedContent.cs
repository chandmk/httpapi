using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace httpapi.web.Helpers
{
    public class CompressedContent : HttpContent
    {
        private HttpContent originalContent;
        private EncodingType encodingType;


        public CompressedContent(HttpContent content, EncodingType encodingType)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            originalContent = content;
            this.encodingType = encodingType;

            // copy the headers from the original content
            foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(encodingType.ToString());
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;
            switch (encodingType)
            {
                case EncodingType.gzip:
                    compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
                    break;
                case EncodingType.deflate:
                    compressedStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
                    break;
            }

            return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
                {
                    if (compressedStream != null)
                    {
                        compressedStream.Dispose();
                    }
                });
        }

        public enum EncodingType
        {
            gzip = 0,
            deflate = 1
        }
    }
}