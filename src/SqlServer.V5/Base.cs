﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.Transport.SQLServer;
using NServiceBus.Compatibility;

class Base : ITestBehavior
{
    public EndpointConfiguration Configure(PluginOptions opts)
    {
        var endpointName = GetType().Name;

        var config = new EndpointConfiguration(opts.ApplyUniqueRunPrefix(endpointName));
        config.EnableInstallers();
        config.UsePersistence<InMemoryPersistence>();

        var transport = config.UseTransport<SqlServerTransport>();
        transport.ConnectionString(opts.ConnectionString + $";App={endpointName}");
        transport.Transactions(TransportTransactionMode.ReceiveOnly);

        config.Conventions().DefiningMessagesAs(t => t.GetInterfaces().Any(x => x.Name == "IMessage"));
        config.Conventions().DefiningCommandsAs(t => t.GetInterfaces().Any(x => x.Name == "ICommand"));
        config.Conventions().DefiningEventsAs(t => t.GetInterfaces().Any(x => x.Name == "IEvent"));

        transport.SubscriptionSettings().SubscriptionTableName(opts.ApplyUniqueRunPrefix("SubscriptionRouting"));

        config.SendFailedMessagesTo(opts.ApplyUniqueRunPrefix("error"));
        config.AuditProcessedMessagesTo(opts.AuditQueue);
        config.AddHeaderToAllOutgoingMessages(nameof(opts.TestRunId), opts.TestRunId);

        Configure(opts, config, transport, transport.Routing());

        return config;
    }

    protected virtual void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig,
        RoutingSettings<SqlServerTransport> routingConfig
    )
    {
    }

    public virtual Task Execute(IEndpointInstance endpointInstance, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}