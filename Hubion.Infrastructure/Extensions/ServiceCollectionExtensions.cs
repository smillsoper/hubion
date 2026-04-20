using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Interfaces.Services;
using Hubion.Application.Services;
using Hubion.Infrastructure.Auth;
using Hubion.Infrastructure.Data;
using Hubion.Infrastructure.FlowEngine;
using Hubion.Infrastructure.FlowEngine.NodeHandlers;
using Hubion.Infrastructure.Repositories;
using Hubion.Infrastructure.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Hubion.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Platform-level DbContext — public schema, tenant table, migrations history
        services.AddDbContext<HubionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Tenant-scoped DbContext — created lazily per request via factory using search_path.
        // NOT registered directly as TenantDbContext in DI — repositories receive
        // ScopedTenantDbContextFactory and call .Create() on first use, after
        // TenantResolutionMiddleware has populated TenantContext.
        services.AddSingleton<ITenantDbContextFactory, TenantDbContextFactory>();
        services.AddScoped<ScopedTenantDbContextFactory>();

        // Tenant context (scoped — holds current request's resolved Tenant)
        services.AddScoped<TenantContext>();

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICallRecordRepository, CallRecordRepository>();
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IFlowRepository, FlowRepository>();
        services.AddScoped<IFlowSessionRepository, FlowSessionRepository>();

        // Services
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Variable resolver (singleton — stateless, thread-safe regex engine)
        services.AddSingleton<IVariableResolver, VariableResolver>();

        // Flow engine node handlers — each registered as INodeHandler so engine
        // receives IEnumerable<INodeHandler> and builds its dispatch dictionary
        services.AddScoped<INodeHandler, ScriptNodeHandler>();
        services.AddScoped<INodeHandler, InputNodeHandler>();
        services.AddScoped<INodeHandler, BranchNodeHandler>();
        services.AddScoped<INodeHandler, SetVariableNodeHandler>();
        services.AddScoped<INodeHandler, ApiCallNodeHandler>();
        services.AddScoped<INodeHandler, EndNodeHandler>();

        // Flow engine (scoped — uses scoped repositories and tenant context)
        services.AddScoped<IFlowEngine, FlowEngine.FlowEngine>();

        // HTTP client for ApiCallNodeHandler
        services.AddHttpClient("FlowEngine");

        // Redis — singleton connection multiplexer shared across all requests
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        return services;
    }
}
