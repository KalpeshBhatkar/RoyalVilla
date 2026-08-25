using RoyalVilla.DTO;

namespace RoyalVillaWeb.Services.IServices
{
    public interface IVillaService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> CreateAsync<T>(CreateVillaDTO dto);
        Task<T?> UpdateAsync<T>(UpdateVillaDTO dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
