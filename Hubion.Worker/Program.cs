using Hubion.Infrastructure.Extensions;
using Hubion.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<SubscriptionProcessingService>();

var host = builder.Build();
host.Run();
