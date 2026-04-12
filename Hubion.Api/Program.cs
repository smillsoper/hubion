using Hubion.Api.Endpoints;
using Hubion.Api.Middleware;
using Hubion.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseTenantResolution();

app.MapTenantsEndpoints();
app.MapCallRecordsEndpoints();

app.Run();
