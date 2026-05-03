using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Application.Interfaces.Services;
using ContactConnection.Application.Services;
using ContactConnection.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ContactConnection.Api.Endpoints;

public static class AgentsEndpoints
{
    public static IEndpointRouteBuilder MapAgentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agents").RequireAuthorization();

        group.MapGet("{id:guid}", GetById);
        group.MapPost("", Create).AllowAnonymous(); // Bootstrap: first agent has no JWT yet

        return app;
    }

    private static async Task<IResult> GetById(
        Guid id,
        IAgentRepository agents,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var agent = await agents.GetByIdAsync(id, ct);
        return agent is null ? Results.NotFound() : Results.Ok(ToResponse(agent));
    }

    private static async Task<IResult> Create(
        CreateAgentRequest request,
        IAgentRepository agents,
        IPasswordHasher hasher,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        if (!AgentRole.IsValid(request.Role))
            return Results.BadRequest(new { error = $"Invalid role '{request.Role}'. Valid: {string.Join(", ", AgentRole.All)}" });

        if (await agents.EmailExistsAsync(request.Email, ct))
            return Results.Conflict(new { error = $"Email '{request.Email}' is already registered." });

        var agent = Agent.Create(
            tenantContext.Current!.Id,
            request.FirstName,
            request.LastName,
            request.Email,
            hasher.Hash(request.Password),
            request.Role);

        await agents.AddAsync(agent, ct);
        await agents.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/agents/{agent.Id}", ToResponse(agent));
    }

    private static object ToResponse(Agent a) => new
    {
        a.Id,
        a.TenantId,
        a.FirstName,
        a.LastName,
        a.Email,
        a.Role,
        a.IsActive,
        a.CreatedAt,
        a.LastLoginAt
        // PasswordHash intentionally excluded
    };
}

public record CreateAgentRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role);
