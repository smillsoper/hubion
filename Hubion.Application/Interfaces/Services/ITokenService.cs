using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(Agent agent, Tenant tenant);
}
