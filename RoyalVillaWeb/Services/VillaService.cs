using RoyalVillaWeb.Services.IServices;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services
{
    public class VillaService : BaseService, IVillaService
    {
        //private readonly string _villaUrl;

        private const string APIEndpoint = $"/api/villa";
        public VillaService(IHttpClientFactory httpClient, IConfiguration configuration) : base(httpClient)
        {
            //_villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public Task<T?> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndpoint}",
                Token = token
            });
        }

        public Task<T?> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{APIEndpoint}/{id}",
                Token = token
            });
        }

        public Task<T?> CreateAsync<T>(CreateVillaDTO dto, string token)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = $"{APIEndpoint}",
                Token = token
            });
        }

        public Task<T?> UpdateAsync<T>(UpdateVillaDTO dto, string token)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{APIEndpoint}/{dto.Id}",
                Token = token
            });
        }

        public Task<T?> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{APIEndpoint}/{id}",
                Token = token
            });
        }
    }
}
