using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface IDataTypeRepository
{
    Task<List<DataType>> GetAllAsync(CancellationToken ct = default);
    Task<DataType?> GetByNameAsync(string typeName, CancellationToken ct = default);
}
