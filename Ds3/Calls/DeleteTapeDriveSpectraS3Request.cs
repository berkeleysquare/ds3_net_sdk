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
using Ds3.Models;
using System;
using System.Net;

namespace Ds3.Calls
{
    public class DeleteTapeDriveSpectraS3Request : Ds3Request
    {
        
        public string TapeDriveId { get; private set; }

        

        
        
        public DeleteTapeDriveSpectraS3Request(Guid tapeDriveId)
        {
            this.TapeDriveId = tapeDriveId.ToString();
            
        }

        
        public DeleteTapeDriveSpectraS3Request(string tapeDriveId)
        {
            this.TapeDriveId = tapeDriveId;
            
        }

        internal override HttpVerb Verb
        {
            get
            {
                return HttpVerb.DELETE;
            }
        }

        internal override string Path
        {
            get
            {
                return "/_rest_/tape_drive/" + TapeDriveId.ToString();
            }
        }
    }
}