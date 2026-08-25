using RoyalVillaWeb.Services.IServices;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services
{
    public class VillaService : BaseService, IVillaService
    {
        //private readonly string _villaUrl;

        private const string APIEndpoint = $"/api/villa";
        public VillaService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
            //_villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndpoint}"
            });
        }

        public Task<T?> GetAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndpoint}/{id}"
            });
        }

        public Task<T?> CreateAsync<T>(CreateVillaDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = $"{APIEndpoint}"
            });
        }

        public Task<T?> UpdateAsync<T>(UpdateVillaDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndpoint}/{dto.Id}"
            });
        }

        public Task<T?> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{APIEndpoint}/{id}"
            });
        }
    }
}
