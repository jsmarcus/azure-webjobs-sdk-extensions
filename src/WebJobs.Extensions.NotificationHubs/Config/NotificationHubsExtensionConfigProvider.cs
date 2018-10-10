// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.NotificationHubs.Bindings;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Azure.WebJobs.Extensions.NotificationHubs
{
    /// <summary>
    /// Defines the configuration options for the NotificationHubs binding.
    /// </summary>
    [Extension("NotificationHubs")]
    internal class NotificationHubsExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly NotificationHubsOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentDictionary<Tuple<string, string>, INotificationHubClientService> _clientCache = new ConcurrentDictionary<Tuple<string, string>, INotificationHubClientService>();        

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        internal NotificationHubsExtensionConfigProvider(IOptions<NotificationHubsOptions> options, ILoggerFactory loggerFactory)
        {
            NotificationHubClientServiceFactory = new DefaultNotificationHubClientServiceFactory();
            _options = options.Value;
            _loggerFactory = loggerFactory;            
        }

        internal INotificationHubClientServiceFactory NotificationHubClientServiceFactory { get; set; }

        /// <inheritdoc />
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var rule = context.AddBindingRule<NotificationHubAttribute>();
            rule.AddNotificationHubConverters();
            rule.BindToInput(new NotificationHubClientBuilder(this));
            rule.BindToCollector((attribute) => BuildFromAttribute(attribute, _loggerFactory.CreateLogger("NotificationHubs")));
        }

        internal IAsyncCollector<Notification> BuildFromAttribute(NotificationHubAttribute attribute, ILogger logger)
        {
            string resolvedConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting);
            string resolvedHubName = ResolveHubName(attribute.HubName);
            bool enableTestSend = attribute.EnableTestSend;

            INotificationHubClientService service = GetService(resolvedConnectionString, resolvedHubName, enableTestSend);
            return new NotificationHubAsyncCollector(service, attribute.TagExpression, attribute.EnableTestSend, logger);
        }

        internal NotificationHubClient BindForNotificationHubClient(NotificationHubAttribute attribute)
        {
            string resolvedConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting);
            string resolvedHubName = ResolveHubName(attribute.HubName);
            INotificationHubClientService service = GetService(resolvedConnectionString, resolvedHubName, attribute.EnableTestSend);

            return service.GetNotificationHubClient();
        }

        internal INotificationHubClientService GetService(string connectionString, string hubName, bool enableTestSend)
        {
            return _clientCache.GetOrAdd(new Tuple<string, string>(connectionString, hubName.ToLowerInvariant()), (c) => NotificationHubClientServiceFactory.CreateService(c.Item1, c.Item2, enableTestSend));
        }

        /// <summary>
        /// If the attribute ConnectionString is not null or empty, the value is looked up in ConnectionStrings, 
        /// AppSettings, and Environment variables, in that order. Otherwise, the config ConnectionString is
        /// returned.
        /// </summary>
        /// <param name="attributeConnectionString">The connection string from the <see cref="NotificationHubAttribute"/>.</param>
        /// <returns></returns>
        internal string ResolveConnectionString(string attributeConnectionString)
        {
            // First, try the Attribute's string.
            if (!string.IsNullOrEmpty(attributeConnectionString))
            {
                return attributeConnectionString;
            }

            // Then use the options.
            return _options.ConnectionString;
        }

        /// <summary>
        /// Returns the attributeHubName, as-is, if it is not null or empty. Because the HubName is not considered
        /// a secret, it can be passed as a string literal without requiring an app setting lookup.
        /// </summary>
        /// <param name="attributeHubName">The hub name from the <see cref="NotificationHubAttribute"/>.</param>
        /// <returns></returns>
        internal string ResolveHubName(string attributeHubName)
        {
            // First, try the Attribute's string.
            if (!string.IsNullOrEmpty(attributeHubName))
            {
                return attributeHubName;
            }

            // Then use the options.
            return _options.HubName;
        }
    }
}
