using System.Collections.Immutable;
using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var statestore = builder.AddDaprStateStore("pizzastatestore");
var pubsubComponent = builder.AddDaprPubSub("pizzapubsub");

builder.AddProject<Projects.PizzaStorefront>("pizzastorefront")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "pizza-storefront",
        DaprHttpPort = 3502,
    })
    .WithReference(pubsubComponent);

builder.AddProject<Projects.PizzaKitchen>("pizzakitchen")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "pizza-kitchen",
        DaprHttpPort = 3503,
    })
    .WithReference(pubsubComponent);

builder.AddProject<Projects.PizzaDelivery>("pizzadelivery")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "pizza-delivery",
        DaprHttpPort = 3504,
    })
    .WithReference(pubsubComponent);

builder.AddProject<Projects.PizzaOrder>("pizzaorder")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "pizza-order",
        DaprHttpPort = 3501,
    })
    .WithReference(statestore)
    .WithReference(pubsubComponent);

builder.AddProject<Projects.PizzaWorkflow>("pizzaworkflow")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "pizza-workflow",
        DaprHttpPort = 3505,
    })
    .WithReference(statestore);

builder.Build().Run();
