﻿using NServiceBus;
using NServiceBus.Compatibility;

class SchemaReceiver : Receiver
{
    public override void Configure(
        PluginOptions opts,
        EndpointConfiguration endpointConfig,
        TransportExtensions<SqlServerTransport> transportConfig
    )
    {
        base.Configure(opts, endpointConfig, transportConfig);

        transportConfig.DefaultSchema(nameof(Receiver));
        transportConfig.UseSchemaForQueue(opts.AuditQueue, "dbo");
    }
}