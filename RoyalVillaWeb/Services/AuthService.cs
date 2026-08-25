using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private const string APIEndpoint = $"/api/auth";
        public AuthService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {

        }

        Task<T?> IAuthService.LoginAsync<T>(LoginRequestDTO loginRequestDTO) where T : default
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = $"{APIEndpoint}/login"
            });
        }

        Task<T?> IAuthService.RegisterAsync<T>(RegisterationRequestDTO registerationRequestDTO) where T : default
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = registerationRequestDTO,
                Url = $"{APIEndpoint}/register"
            });
        }
    }
}
