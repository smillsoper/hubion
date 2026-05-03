using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Application.Interfaces.Services;
using ContactConnection.Application.Services;

namespace ContactConnection.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("login", Login);

        return app;
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAgentRepository agents,
        ITenantRepository tenants,
        IPasswordHasher hasher,
        ITokenService tokens,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var agent = await agents.GetByEmailAsync(request.Email, ct);

        // Identical response for unknown email and wrong password — no enumeration
        if (agent is null || !hasher.Verify(request.Password, agent.PasswordHash))
            return Results.Unauthorized();

        agent.RecordLogin();
        await agents.SaveChangesAsync(ct);

        var token = tokens.GenerateToken(agent, tenantContext.Current!);

        return Results.Ok(new LoginResponse(
            token,
            agent.Id,
            agent.Email,
            agent.FirstName,
            agent.LastName,
            agent.Role,
            tenantContext.Current!.Subdomain));
    }
}

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    Guid AgentId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string TenantSubdomain);
