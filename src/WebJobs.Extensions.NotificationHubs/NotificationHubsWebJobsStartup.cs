// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.NotificationHubs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(NotificationHubsWebJobsStartup))]

namespace Microsoft.Azure.WebJobs.Extensions.NotificationHubs
{
    public class NotificationHubsWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.UseNotificationHubs();
        }
    }
}
