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

namespace Microsoft.WindowsAzure.Management.Test.Environment
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddAzureEnvironmentTests : TestBase
    {
        private FileSystemHelper helper;

        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            helper = new FileSystemHelper(this);
            helper.CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestCleanup]
        public void Cleanup()
        {
            helper.Dispose();
        }

        [TestMethod]
        public void AddsAzureEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            AddAzureEnvironmentCommand cmdlet = new AddAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = "Katal",
                PublishSettingsFileUrl = "http://microsoft.com",
                ServiceEndpoint = "endpoint",
                ManagementPortalUrl = "management portal url",
                StorageBlobEndpointFormat = "blob format",
                StorageQueueEndpointFormat = "queue format",
                StorageTableEndpointFormat = "table format"
            };

            cmdlet.ExecuteCmdlet();

            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<WindowsAzureEnvironment>()), Times.Once());
            WindowsAzureEnvironment env = GlobalSettingsManager.Instance.GetEnvironment("KaTaL");
            Assert.AreEqual(env.Name, cmdlet.Name);
            Assert.AreEqual(env.PublishSettingsFileUrl, cmdlet.PublishSettingsFileUrl);
            Assert.AreEqual(env.ServiceEndpoint, cmdlet.ServiceEndpoint);
            Assert.AreEqual(env.ManagementPortalUrl, cmdlet.ManagementPortalUrl);
            Assert.AreEqual(env.StorageBlobEndpointFormat, cmdlet.StorageBlobEndpointFormat);
            Assert.AreEqual(env.StorageQueueEndpointFormat, cmdlet.StorageQueueEndpointFormat);
            Assert.AreEqual(env.StorageTableEndpointFormat, cmdlet.StorageTableEndpointFormat);
        }

        [TestMethod]
        public void AddsEnvironmentWithMinimumInformation()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            AddAzureEnvironmentCommand cmdlet = new AddAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = "Katal",
                PublishSettingsFileUrl = "http://microsoft.com"
            };

            cmdlet.ExecuteCmdlet();

            commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<WindowsAzureEnvironment>()), Times.Once());
            WindowsAzureEnvironment env = GlobalSettingsManager.Instance.GetEnvironment("KaTaL");
            Assert.AreEqual(env.Name, cmdlet.Name);
            Assert.AreEqual(env.PublishSettingsFileUrl, cmdlet.PublishSettingsFileUrl);
        }

        [TestMethod]
        public void IgnoresAddingDuplicatedEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            AddAzureEnvironmentCommand cmdlet = new AddAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = "Katal",
                PublishSettingsFileUrl = "http://microsoft.com",
                ServiceEndpoint = "endpoint",
                ManagementPortalUrl = "management portal url",
                StorageBlobEndpointFormat = "blob format",
                StorageQueueEndpointFormat = "queue format",
                StorageTableEndpointFormat = "table format"
            };
            cmdlet.ExecuteCmdlet();
            int count = GlobalSettingsManager.Instance.GetEnvironments().Count;

            // Add again
            cmdlet.Name = "kAtAl";
            Testing.AssertThrows<Exception>(() => cmdlet.ExecuteCmdlet());
        }

        [TestMethod]
        public void IgnoresAddingPublicEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            AddAzureEnvironmentCommand cmdlet = new AddAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = EnvironmentName.AzureCloud,
                PublishSettingsFileUrl = "http://microsoft.com"
            };
            int count = GlobalSettingsManager.Instance.GetEnvironments().Count;

            Testing.AssertThrows<Exception>(() => cmdlet.ExecuteCmdlet());
        }
    }
}