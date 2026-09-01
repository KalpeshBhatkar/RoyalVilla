using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RoyalVillaWeb.Services
{
    public class BaseService : IBaseService
    {
        public IHttpClientFactory _httpClient { get; set; }
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _httpcontextAccessor;
        private const string RefreshingTokenKey = "_RefreshingToken";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public ApiResponse<object> ResponseModel { get; set; }
        public BaseService(IHttpClientFactory httpClient, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
        {
            this.ResponseModel = new();
            this._httpClient = httpClient;
            this._tokenProvider = tokenProvider;
            this._httpcontextAccessor = httpContextAccessor;
        }

        private bool IsRefreshingToken
        {
            get => _httpcontextAccessor.HttpContext?.Session.GetString(RefreshingTokenKey) == "true";
            set
            {
                if (value)
                {
                    _httpcontextAccessor.HttpContext?.Session.SetString(RefreshingTokenKey, "true");
                }
                else
                {
                    _httpcontextAccessor.HttpContext?.Session.Remove(RefreshingTokenKey);
                }
            }
        }

        public async Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true)
        {
            try
            {
                var client = _httpClient.CreateClient("RoyalVillaAPI");
                var message = CreateRequestMessage(apiRequest, withBearer);
                var apiResponse = await client.SendAsync(message);

                if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && withBearer && !IsRefreshingToken)
                {
                    // Handle unauthorized access, e.g., refresh token or redirect to login
                    Console.WriteLine("Unauthorized access. Please check your credentials.");
                    var refreshed = await RefreshAccessTokenAsync();
                    if (refreshed)
                    {
                        var retrymessage = CreateRequestMessage(apiRequest, withBearer);
                        apiResponse = await client.SendAsync(retrymessage);
                    }
                    else
                    {
                        _tokenProvider.ClearToken();
                        await _httpcontextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        _httpcontextAccessor.HttpContext?.Response.Redirect("/auth/login");
                        return default;
                    }
                }

                return await apiResponse.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error : {ex.Message}");
                return default;
            }
        }

        private static HttpMethod GetHttpMethod(SD.ApiType apiType)
        {
            return apiType switch
            {
                SD.ApiType.POST => HttpMethod.Post,
                SD.ApiType.PUT => HttpMethod.Put,
                SD.ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };
        }

        private HttpRequestMessage CreateRequestMessage(ApiRequest apiRequest, bool withBearer = true)
        {
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(apiRequest.Url, uriKind: UriKind.Relative),
                Method = GetHttpMethod(apiRequest.ApiType)
            };

            if (withBearer)
            {
                var token = _tokenProvider.GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            if (apiRequest.Data != null)
            {
                if (apiRequest.Data is MultipartFormDataContent multipartFormDataContent)
                {
                    message.Content = multipartFormDataContent;
                }
                else
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: JsonOptions);
                }
            }

            return message;
        }

        private async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                if (IsRefreshingToken)
                {
                    await Task.Delay(1000);
                    var accessToken = _tokenProvider.GetAccessToken();

                    if (accessToken != null) { return true; }
                    return false;
                }

                IsRefreshingToken = true;

                var refreshToken = _tokenProvider.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }
                var client = _httpClient.CreateClient("RoyalVillaAPI");
                var refreshRequest = new RefreshTokenRequestDTO
                {
                    RefreshToken = refreshToken
                };
                var apiRequest = new ApiRequest
                {
                    ApiType = SD.ApiType.POST,
                    Data = refreshRequest,
                    Url = $"/api/auth/refresh-token"
                };

                var message = CreateRequestMessage(apiRequest, withBearer: false);
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenDTO>>();
                    if (result?.Success == true && result.Data != null && !string.IsNullOrEmpty(result.Data.AccessToken) && !string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        _tokenProvider.SetToken(result.Data.AccessToken, result.Data.RefreshToken);
                        return true;
                    }
                }

                _tokenProvider.ClearToken();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token refresh failed: " + ex.Message);
                _tokenProvider.ClearToken();
                return false;
            }
            finally
            {
                IsRefreshingToken = false;
            }
        }

    }
}
