using Hubion.Application.Interfaces.Repositories;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class DataTypeRepository : IDataTypeRepository
{
    private readonly HubionDbContext _ctx;

    public DataTypeRepository(HubionDbContext ctx) => _ctx = ctx;

    public Task<List<DataType>> GetAllAsync(CancellationToken ct = default)
        => _ctx.DataTypes.OrderBy(d => d.TypeName).ToListAsync(ct);

    public Task<DataType?> GetByNameAsync(string typeName, CancellationToken ct = default)
        => _ctx.DataTypes.FirstOrDefaultAsync(d => d.TypeName == typeName, ct);
}
