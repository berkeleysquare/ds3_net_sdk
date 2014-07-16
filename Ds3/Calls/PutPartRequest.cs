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

using System.IO;

namespace Ds3.Calls
{
    public class PutPartRequest : Ds3Request
    {
        private readonly Stream _content;
        public string BucketName { get; private set; }
        public string ObjectName { get; private set; }
        public int PartNumber { get; private set; }
        public string UploadId { get; private set; }

        public PutPartRequest(string bucketName, string objectName, int partNumber, string uploadId, Stream content)
        {
            this.BucketName = bucketName;
            this.ObjectName = objectName;
            this.PartNumber = partNumber;
            this.UploadId = uploadId;
            this.QueryParams.Add("partNumber", partNumber.ToString());
            this.QueryParams.Add("uploadId", uploadId);
            this._content = content;
        }

        internal override HttpVerb Verb
        {
            get { return HttpVerb.PUT; }
        }

        internal override string Path
        {
            get { return "/" + BucketName + "/" + ObjectName; }
        }

        internal override Stream GetContentStream()
        {
            return _content;
        }
    }
}
