/*
 * ******************************************************************************
 *   Copyright 2014-2017 Spectra Logic Corporation. All Rights Reserved.
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

// This code is auto-generated, do not modify

using Ds3.Calls;
using Ds3.Models;
using Ds3.Runtime;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace Ds3.ResponseParsers
{
    internal class GetJobsSpectraS3ResponseParser : IResponseParser<GetJobsSpectraS3Request, GetJobsSpectraS3Response>
    {
        public GetJobsSpectraS3Response Parse(GetJobsSpectraS3Request request, IWebResponse response)
        {
            using (response)
            {
                ResponseParseUtilities.HandleStatusCode(response, (HttpStatusCode)200);
                using (var stream = response.GetResponseStream())
                {
                    return new GetJobsSpectraS3Response(
                        ModelParsers.ParseJobList(
                            XmlExtensions.ReadDocument(stream).ElementOrThrow("Jobs"))
                    );
                }
            }
        }
    }
}