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

namespace Microsoft.WindowsAzure.Commands.Storage.Blob
{
    using Microsoft.WindowsAzure.Commands.Storage.Common;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using Microsoft.WindowsAzure.Storage.DataMovement.TransferJobs;
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Threading.Tasks;

    public class StorageDataMovementCmdletBase : StorageCloudBlobCmdletBase, IDisposable
    {
        /// <summary>
        /// Blob Transfer Manager
        /// </summary>
        private DataManagementWrapper dataManagementWrapper;

        [Parameter(HelpMessage = "Force to overwrite the existing blob or file")]
        public SwitchParameter Force
        {
            get { return overwrite; }
            set { overwrite = value; }
        }

        protected bool overwrite;

        /// <summary>
        /// Confirm the overwrite operation
        /// </summary>
        /// <param name="msg">Confirmation message</param>
        /// <returns>True if the opeation is confirmed, otherwise return false</returns>
        private bool ConfirmOverwrite(string sourcePath, string destinationPath)
        {
            string overwriteMessage = String.Format(Resources.OverwriteConfirmation, destinationPath);
            return overwrite || OutputStream.ConfirmAsync(overwriteMessage).Result;
        }

        /// <summary>
        /// On Task run successfully
        /// </summary>
        /// <param name="data">User data</param>
        protected virtual void OnTaskSuccessful(DataMovementUserData data)
        { }


        /// <summary>
        /// Cmdlet begin processing
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.dataManagementWrapper = new DataManagementWrapper(this.GetCmdletConcurrency(), CmdletOperationContext.ClientRequestId);
        }

        protected async Task RunTransferJob(BlobTransferJob transferJob, DataMovementUserData userData)
        {
            this.SetRequestOptionsInTransferJob(transferJob);
            transferJob.OverwritePromptCallback = ConfirmOverwrite;

            try
            {
                await this.dataManagementWrapper.RunTransferJob(
                    transferJob,
                    (percent, speed) =>
                    {
                        userData.Record.PercentComplete = (int)percent;
                        userData.Record.StatusDescription = String.Format(Resources.FileTransmitStatus, (int)percent, Util.BytesToHumanReadableSize(speed));
                        this.OutputStream.WriteProgress(userData.Record);
                    },
                    this.CmdletCancellationToken);

                userData.Record.PercentComplete = 100;
                userData.Record.StatusDescription = Resources.TransmitSuccessfully;
            }
            catch (Exception e)
            {
                userData.Record.StatusDescription = String.Format(Resources.TransmitFailed, e.Message);
                this.OutputStream.WriteProgress(userData.Record);
                throw;
            }

            this.OutputStream.WriteProgress(userData.Record);
        }

        protected void SetRequestOptionsInTransferJob(BlobTransferJob transferJob)
        {
            BlobRequestOptions cmdletOptions = RequestOptions;

            if (cmdletOptions == null)
            {
                return;
            }

            BlobRequestOptions requestOptions = transferJob.BlobRequestOptions;

            if (cmdletOptions.MaximumExecutionTime != null)
            {
                requestOptions.MaximumExecutionTime = cmdletOptions.MaximumExecutionTime;
            }

            if (cmdletOptions.ServerTimeout != null)
            {
                requestOptions.ServerTimeout = cmdletOptions.ServerTimeout;
            }

            transferJob.BlobRequestOptions = requestOptions;
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteTaskSummary();
        }

        /// <summary>
        /// Dispose DataMovement cmdlet
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose DataMovement cmdlet
        /// </summary>
        /// <param name="disposing">User disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.dataManagementWrapper != null)
                {
                    this.dataManagementWrapper.Dispose();
                    this.dataManagementWrapper = null;
                }
            }
        }
    }
}
