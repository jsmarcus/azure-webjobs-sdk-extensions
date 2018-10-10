// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.NotificationHubs
{
    public class NotificationHubsOptions
    {
        public string ConnectionString { get; set; }

        public string HubName { get; set; }
    }
}
