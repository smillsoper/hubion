using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class DataTypeRepository : IDataTypeRepository
{
    private readonly ContactConnectionDbContext _ctx;

    public DataTypeRepository(ContactConnectionDbContext ctx) => _ctx = ctx;

    public Task<List<DataType>> GetAllAsync(CancellationToken ct = default)
        => _ctx.DataTypes.OrderBy(d => d.TypeName).ToListAsync(ct);

    public Task<DataType?> GetByNameAsync(string typeName, CancellationToken ct = default)
        => _ctx.DataTypes.FirstOrDefaultAsync(d => d.TypeName == typeName, ct);
}
