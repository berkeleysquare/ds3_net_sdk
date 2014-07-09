﻿/*
 * ******************************************************************************
 *   Copyright 2014 Spectra Logic Corporation. All Rights Reserved.
 *   Licensed under the Apache License, Version 2.0 (the "License"). You may not use
 *   this file except in compliance with the License. A copy of the License is located at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 *   or in the "license" file accompanying this file.
 *   This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 *   CONDITIONS OF ANY KIND, either express or implied. See the License for the
 *   specific language governing permissions and limitations under the License.
 * ****************************************************************************
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Ds3.Models;
using Ds3.Calls;

namespace Ds3.Runtime
{
    internal class Network : INetwork
    {
        internal const int DefaultCopyBufferSize = 1 * 1024 * 1024;

        private readonly Uri _endpoint;
        private readonly Credentials _creds;
        private readonly int _maxRedirects = 0;
        private readonly int _redirectRetryCount;
        private readonly int _readWriteTimeout;
        private readonly int _requestTimeout;

        internal Uri Proxy = null;

        public Network(
            Uri endpoint,
            Credentials creds,
            int redirectRetryCount,
            int copyBufferSize,
            int readWriteTimeout,
            int requestTimeout)
        {
            this._endpoint = endpoint;
            this._creds = creds;
            this._redirectRetryCount = redirectRetryCount;
            this.CopyBufferSize = copyBufferSize;
            this._readWriteTimeout = readWriteTimeout;
            this._requestTimeout = requestTimeout;
        }

        public int CopyBufferSize { get; private set; }

        public IWebResponse Invoke(Ds3Request request)
        {
            bool redirect = false;
            int redirectCount = 0;            
            
            using (var content = request.GetContentStream())
            {
                do
                {
                    HttpWebRequest httpRequest = CreateRequest(request, content);
                    try
                    {
                        var response = new WebResponse((HttpWebResponse)httpRequest.GetResponse());
                        if (Is307(response))
                        {
                            redirect = true;
                            redirectCount++;
                            Trace.Write(string.Format(Resources.Encountered307NTimes, redirectCount), "Ds3Network");
                            continue;
                        }
                        return response;
                    }
                    catch (WebException e)
                    {
                        if (e.Response == null)
                        {
                            throw e;
                        }
                        return new WebResponse((HttpWebResponse)e.Response);
                    }
                } while (redirect && redirectCount < _maxRedirects);
            }

            throw new Ds3RedirectLimitException(Resources.TooManyRedirectsException);
        }

        private HttpWebRequest CreateRequest(Ds3Request request, Stream content)
        {
            DateTime date = DateTime.UtcNow;
            UriBuilder uriBuilder = new UriBuilder(_endpoint);
            uriBuilder.Path = request.Path;

            if (request.QueryParams.Count > 0)
            {
                uriBuilder.Query = BuildQueryParams(request.QueryParams);
            }

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
            httpRequest.Method = request.Verb.ToString();
            if (Proxy != null)
            {
                WebProxy webProxy = new WebProxy();
                webProxy.Address = Proxy;
                httpRequest.Proxy = webProxy;
            }
            httpRequest.Date = date;
            httpRequest.Host = CreateHostString(_endpoint);
            httpRequest.AllowAutoRedirect = false;
            httpRequest.AllowWriteStreamBuffering = false;
            httpRequest.ReadWriteTimeout = this._readWriteTimeout;
            httpRequest.Timeout = this._requestTimeout;
            
            var md5 = ComputeChecksum(request.Md5, content);
            if (!string.IsNullOrEmpty(md5))
            {
                httpRequest.Headers.Add(HttpHeaders.ContentMd5, md5);
            }
            httpRequest.Headers.Add(HttpHeaders.Authorization, S3Signer.AuthField(
                _creds,
                request.Verb.ToString(),
                date.ToString("r"),
                request.Path,
                md5: md5,
                amzHeaders: request.Headers
            ));

            var byteRange = request.GetByteRange();
            if (byteRange != null)
            {
                httpRequest.AddRange(byteRange.Start, byteRange.End);
            }
            
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            if (request.Verb == HttpVerb.PUT || request.Verb == HttpVerb.POST)
            {
                httpRequest.ContentLength = content.Length;
                using (var requestStream = httpRequest.GetRequestStream())
                {
                    if (content != Stream.Null)
                    {
                        if (!content.CanSeek || !content.CanRead)
                        {
                            throw new Ds3.Runtime.Ds3RequestException(Resources.InvalidStreamException);
                        }
                        content.Seek(0, SeekOrigin.Begin);
                        content.CopyTo(requestStream, this.CopyBufferSize);
                        requestStream.Flush();
                    }
                }
            }
            return httpRequest;
        }

        private static string ComputeChecksum(Checksum checksum, Stream content)
        {
            return checksum.Match(
                () => "",
                () => Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(content)),
                hash => Convert.ToBase64String(hash)
            );
        }

        private string CreateHostString(Uri endpoint)
        {
            if(endpoint.Port > 0) {
                return endpoint.Host + ":" + endpoint.Port;
            }
            return endpoint.Host;
        }

        private bool Is307(IWebResponse httpResponse)
        {
            return httpResponse.StatusCode.Equals(HttpStatusCode.TemporaryRedirect);
        }

        private string FormatedDateString()
        {
            return DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss K");
        }

        private string BuildQueryParams(Dictionary<string, string> queryParams)
        {
            List<string> queryList = queryParams.Select(kvp => kvp.Key + "=" + kvp.Value).ToList();
            return String.Join("&", queryList);            
        }
    }
}
