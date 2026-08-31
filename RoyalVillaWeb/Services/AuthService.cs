using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private const string APIEndpoint = $"/api/auth";
        public AuthService(IHttpClientFactory httpClient, IConfiguration configuration, ITokenProvider tokenProvider) : base(httpClient, tokenProvider)
        {

        }

        Task<T?> IAuthService.LoginAsync<T>(LoginRequestDTO loginRequestDTO) where T : default
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = $"{APIEndpoint}/login"
            }, withBearer: false);
        }

        Task<T?> IAuthService.RegisterAsync<T>(RegisterationRequestDTO registerationRequestDTO) where T : default
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = registerationRequestDTO,
                Url = $"{APIEndpoint}/register"
            }, withBearer: false);
        }

        Task<T?> IAuthService.RefreshTokenAsync<T>(RefreshTokenRequestDTO refreshTokenRequestDTO) where T : default
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = refreshTokenRequestDTO,
                Url = $"{APIEndpoint}/refresh-token"
            }, withBearer: false);
        }
    }
}
