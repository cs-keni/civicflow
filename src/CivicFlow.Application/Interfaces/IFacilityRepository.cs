using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Interfaces;

public interface IFacilityRepository
{
    Task<Facility?> GetByIdAsync(int id);
    Task<List<Facility>> GetAllAsync(int page, int pageSize, bool activeOnly = false);
    Task<List<Facility>> GetByOwnerIdAsync(string ownerId, int page, int pageSize);
    Task<int> CountAsync(bool activeOnly = false);
    Task<int> CountByOwnerIdAsync(string ownerId);
    Task<Facility> AddAsync(Facility facility);
    Task UpdateAsync(Facility facility);
}
