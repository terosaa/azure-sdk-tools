﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.ActiveDirectory.Models;
using Microsoft.Azure.Commands.Resources.Models.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ActiveDirectory
{
    /// <summary>
    /// Get AD groups.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureADGroup", DefaultParameterSetName = ParameterSet.Empty), OutputType(typeof(List<PSADGroup>))]
    public class GetAzureADGroupCommand : ActiveDirectoryBaseCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSet.DisplayName,
            HelpMessage = "The user or group name.")]
        [ValidateNotNullOrEmpty]
        public string DisplayName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSet.ObjectId,
            HelpMessage = "The user or group id.")]
        [ValidateNotNullOrEmpty]
        public Guid ObjectId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSet.Empty,
            HelpMessage = "The Use email address.")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = ParameterSet.Email,
            HelpMessage = "The Use email address.")]
        [ValidateNotNullOrEmpty]
        public string Email { get; set; }

        public override void ExecuteCmdlet()
        {
            ADObjectFilterOptions options = new ADObjectFilterOptions
            {
                DisplayName = DisplayName,
                Email = Email,
                Id = ObjectId == Guid.Empty ? null : ObjectId.ToString(),
                Paging = true
            };

            do
            {
                WriteObject(ActiveDirectoryClient.FilterGroups(options), true);
            } while (!string.IsNullOrEmpty(options.NextLink));
        }
    }
}