// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.NotificationHubs;
using Microsoft.Azure.WebJobs.Extensions.Tests.Common;
using Microsoft.Azure.WebJobs.Host.Indexers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NotificationHubsTests
{
    [Trait("Category", "E2E")]
    public class NotificationHubsEndToEndTests
    {
        private const string AttributeConnStr = "Endpoint=sb://AttrTestNS.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=RandomKey";
        private const string AttributeHubName = "AttributeHubName";
        private const string ConfigConnStr = "Endpoint=sb://ConfigTestNS.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=RandomKey";
        private const string ConfigHubName = "ConfigTestHub";
        private const string DefaultConnStr = "Endpoint=sb://DefaultTestNS.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=RandomKey";
        private const string DefaultHubName = "defaulttesthub";
        private const string MessagePropertiesJSON = "{\"message\":\"Hello\",\"location\":\"Redmond\"}";
        private const string WindowsToastPayload = "<toast><visual><binding template=\"ToastText01\"><text id=\"1\">Test message</text></binding></visual></toast>";
        private const string UserIdTag = "myuserid123";
        private static Notification testNotification = Converter.BuildTemplateNotificationFromJsonString(MessagePropertiesJSON);

        private const string DefaultConnectionString = "Endpoint=sb://TestNS.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=2XgXnw2bVCd7GT9RPaZ/RandomKey";
        private const string ConfigConnectionString = "ConfigConnection";
        private const string AttributeConnectionString1 = "AttributeConnection1";
        private const string AttributeConnectionString2 = "AttributeConnection2";

        //private const string DefaultHubName = "TestHub";
        //private const string ConfigHubName = "ConfigHub";
        private const string AttributeHubName1 = "AttributeHub1";
        private const string AttributeHubName2 = "AttributeHub2";

        private readonly TestLoggerProvider _loggerProvider = new TestLoggerProvider();

        //[Fact]
        //public async Task OutputBindings_WithKeysOnConfigAndAttribute()
        //{
        //    string functionName = nameof(NotificationHubsEndToEndFunctions.Outputs_AttributeAndConfig);
        //    InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

        //    await RunTestAsync(functionName, factoryMock.Object, configConnectionString: ConfigApiKey, includeDefaultConnectionString: false);

        //    // We expect three clients to be created. The others should be re-used because the ApiKeys match.
        //    factoryMock.Verify(f => f.CreateService(AttributeConnectionString1, AttributeHubName1, true), Times.Once());
        //    factoryMock.Verify(f => f.CreateService(AttributeConnectionString2, AttributeHubName2, true), Times.Once());
        //    factoryMock.Verify(f => f.CreateService(ConfigConnectionString, ConfigHubName, true), Times.Once());

        //    // This function sends 4 messages.
        //    clientMock.Verify(c => c.SendNotificationAsync(It.IsAny<Notification>(), It.IsAny<string>()), Times.Exactly(4));

        //    // Just make sure traces work.
        //    Assert.Equal(functionName, _loggerProvider.GetAllUserLogMessages().Single().FormattedMessage);

        //    factoryMock.VerifyAll();
        //    clientMock.VerifyAll();
        //}

        //[Fact]
        //public async Task OutputBindings_WithNameResolver()
        //{
        //    string functionName = nameof(NotificationHubsEndToEndFunctions.Outputs);
        //    InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

        //    await RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: true, includeDefaultHubName: true);

        //    // We expect one client to be created.
        //    factoryMock.Verify(f => f.CreateService(ConfigConnStr, ConfigHubName, false), Times.Once());

        //    // This function sends 1 message.
        //    clientMock.Verify(c => c.SendNotificationAsync(It.IsAny<Notification>(), It.IsAny<string>()), Times.Once());

        //    // Just make sure traces work.
        //    Assert.Equal(functionName, _loggerProvider.GetAllUserLogMessages().Single().FormattedMessage);

        //    factoryMock.VerifyAll();
        //    clientMock.VerifyAll();
        //}

        [Fact]
        public async Task Outputs_NoConnectionString()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.Outputs);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: false, includeDefaultHubName: true));

            Assert.Equal("The Notification Hub Connection String must be set either via an 'NotificationHubs' app setting, via an 'NotificationHubs' environment variable, or directly in code via NotificationHubsOptions.ConnectionString or NotificationHubAttribute.ConnectionStringSetting.", ex.InnerException.Message);
        }

        [Fact]
        public async Task Outputs_NoHubName()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.Outputs);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: true, includeDefaultHubName: false));

            Assert.Equal("The Notification Hub Name must be set directly in code via NotificationHubsOptions.HubName or NotificationHubAttribute.HubName.", ex.InnerException.Message);
        }

        [Fact]
        public async Task OutputsAsyncCollector_NoConnectionString()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.OutputsAsyncCollector);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: false, includeDefaultHubName: true));

            Assert.Equal("The Notification Hub Connection String must be set either via an 'NotificationHubs' app setting, via an 'NotificationHubs' environment variable, or directly in code via NotificationHubsOptions.ConnectionString or NotificationHubAttribute.ConnectionStringSetting.", ex.InnerException.Message);
        }

        [Fact]
        public async Task OutputsAsyncCollector_NoHubName()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.OutputsAsyncCollector);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: true, includeDefaultHubName: false));

            Assert.Equal("The Notification Hub Name must be set directly in code via NotificationHubsOptions.HubName or NotificationHubAttribute.HubName.", ex.InnerException.Message);
        }

        [Fact]
        public async Task Client_NoConnectionString()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.Client);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: false, includeDefaultHubName: true));

            Assert.Equal("The Notification Hub Connection String must be set either via an 'NotificationHubs' app setting, via an 'NotificationHubs' environment variable, or directly in code via NotificationHubsOptions.ConnectionString or NotificationHubAttribute.ConnectionStringSetting.", ex.InnerException.Message);
        }

        [Fact]
        public async Task Client_NoHubName()
        {
            string functionName = nameof(NotificationHubsEndToEndFunctions.Client);
            InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configConnectionString: null, includeDefaultConnectionString: true, includeDefaultHubName: false));

            Assert.Equal("The Notification Hub Name must be set directly in code via NotificationHubsOptions.HubName or NotificationHubAttribute.HubName.", ex.InnerException.Message);
        }

        private void InitializeMocks(out Mock<INotificationHubClientServiceFactory> factoryMock, out Mock<INotificationHubClientService> clientMock)
        {
            var mockResponse = new NotificationOutcome();
            clientMock = new Mock<INotificationHubClientService>(MockBehavior.Strict);
            clientMock
                .Setup(c => c.SendNotificationAsync(It.IsAny<Notification>(), It.IsAny<string>()))
                .ReturnsAsync(mockResponse);

            factoryMock = new Mock<INotificationHubClientServiceFactory>(MockBehavior.Strict);
            factoryMock
                .Setup(f => f.CreateService(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(clientMock.Object);
        }

        private async Task RunTestAsync(string testName, INotificationHubClientServiceFactory factory, object argument = null, string configConnectionString = null, bool includeDefaultConnectionString = true, bool includeDefaultHubName = true)
        {
            Type testType = typeof(NotificationHubsEndToEndFunctions);
            var locator = new ExplicitTypeLocator(testType);
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(_loggerProvider);

            var arguments = new Dictionary<string, object>();
            arguments.Add("triggerData", argument);

            var resolver = new TestNameResolver();
            resolver.Values.Add("HubName", "ResolvedHubName");
            resolver.Values.Add("MyConnectionString", AttributeConnStr);

            IHost host = new HostBuilder()
                .ConfigureWebJobs(builder =>
                {
                    builder.UseNotificationHubs();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<INotificationHubClientServiceFactory>(factory);
                    services.AddSingleton<INameResolver>(resolver);
                    services.AddSingleton<ITypeLocator>(locator);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(_loggerProvider);
                })
                .ConfigureAppConfiguration(c =>
                {
                    c.Sources.Clear();
                    var collection = new Dictionary<string, string>
                    {
                        { "MyConnection1", AttributeConnectionString1 },
                        { "MyConnection2", AttributeConnectionString2 }
                    };

                    if (includeDefaultConnectionString)
                    {
                        collection.Add($"ConnectionStrings:{Constants.DefaultConnectionStringName}", DefaultConnectionString);
                    }

                    if (includeDefaultHubName)
                    {
                        collection.Add("AzureWebJobs:extensions:NotificationHubs:HubName", DefaultHubName);
                    }

                    c.AddInMemoryCollection(collection);
                })
                .Build();

            await host.GetJobHost().CallAsync(testType.GetMethod(testName), arguments);
        }

        private class NotificationHubsEndToEndFunctions
        {
            [NoAutomaticTrigger]
            public static void Outputs(
                [NotificationHub] out Notification notification,
                [NotificationHub(TagExpression = "tag")] out Notification notificationToTag,
                [NotificationHub] out WindowsNotification windowsNotification,
                [NotificationHub(Platform = NotificationPlatform.Wns)] out string windowsToastNotificationAsString,
                [NotificationHub] out TemplateNotification templateNotification,
                [NotificationHub] out string templateProperties,
                [NotificationHub] out Notification[] notificationsArray,
                [NotificationHub] out IDictionary<string, string> templatePropertiesDictionary,
                [NotificationHub] ICollector<Notification> collector,
                [NotificationHub] ICollector<string> collectorString,
                ILogger logger)
            {
                notification = GetTemplateNotification("Hi");
                notificationToTag = GetTemplateNotification("Hi tag");
                windowsNotification = new WindowsNotification(WindowsToastPayload);
                windowsToastNotificationAsString = WindowsToastPayload;
                templateNotification = GetTemplateNotification("Hello");
                templateProperties = MessagePropertiesJSON;
                templatePropertiesDictionary = GetTemplateProperties("Hello");
                notificationsArray = new TemplateNotification[]
                {
                    GetTemplateNotification("Message1"),
                    GetTemplateNotification("Message2")
                };
                collector.Add(GetTemplateNotification("Hi"));
                collectorString.Add(MessagePropertiesJSON);
                logger.LogWarning(nameof(Outputs));
            }

            [NoAutomaticTrigger]
            public static async Task OutputsAsyncCollector(
                [NotificationHub] IAsyncCollector<TemplateNotification> asyncCollector,
                [NotificationHub] IAsyncCollector<string> asyncCollectorString,
                ILogger logger)
            {
                await asyncCollector.AddAsync(GetTemplateNotification("Hello"));
                await asyncCollectorString.AddAsync(MessagePropertiesJSON);
                logger.LogWarning(nameof(OutputsAsyncCollector));
            }

            [NoAutomaticTrigger]
            public static async Task Client(
                [NotificationHub] NotificationHubClient client,
                ILogger logger)
            {
                await client.SendNotificationAsync(GetTemplateNotification("Hello"));
                logger.LogWarning(nameof(Client));
            }

            private static TemplateNotification GetTemplateNotification(string msg)
            {
                Dictionary<string, string> templateProperties = new Dictionary<string, string>();
                templateProperties["message"] = msg;
                return new TemplateNotification(templateProperties);
            }

            private static IDictionary<string, string> GetTemplateProperties(string message)
            {
                Dictionary<string, string> templateProperties = new Dictionary<string, string>();
                templateProperties["message"] = message;
                return templateProperties;
            }
        }
    }
}
