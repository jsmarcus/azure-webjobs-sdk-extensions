// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.NotificationHubs
{
    /// <summary>
    /// Extension methods for NotificationHubs integration.
    /// </summary>
    public static class NotificationHubsWebJobsBuilderExtensions
    {
        /// <summary>
        /// Enables use of NotificationHubs extension
        /// </summary>
        public static void UseNotificationHubs(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<NotificationHubsExtensionConfigProvider>()
                .ConfigureOptions<NotificationHubsOptions>((config, path, options) =>
                {
                    options.ConnectionString = config.GetConnectionString(Constants.DefaultConnectionStringName);

                    IConfigurationSection section = config.GetSection(path);
                    section.Bind(options);
                });
        }
    }
}
