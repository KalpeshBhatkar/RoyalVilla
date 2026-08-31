using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody]RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (registerationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration data is required."));
                }

                if (await _authService.IsEmailExistsAsync(registerationRequestDTO.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registerationRequestDTO.Email}' already exists."));
                }

                var user = await _authService.RegisterAsync(registerationRequestDTO);

                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration failed."));
                }

                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"An unexpected error occurred during user registration", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LoginRequestDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginRequestDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required."));
                }

                var loginResponse = await _authService.LoginAsync(loginRequestDTO);

                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login failed. Please check your credentials."));
                }

                var response = ApiResponse<TokenDTO>.Ok(loginResponse, "Login successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"An unexpected error occurred during user login", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<TokenDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<TokenDTO>>> RefreshAccessToken([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (refreshTokenRequestDTO == null || string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Refresh Token is required."));
                }

                var tokenResponse = await _authService.RefreshAccessTokenAsync(refreshTokenRequestDTO);

                if(tokenResponse == null)
                {
                    // Token reuse or invalid token - log for security monitoring
                    var errorResponse = ApiResponse<object>.Error(StatusCodes.Status401Unauthorized,
                        "Invalid or expired refresh token. If token reuse was detected, all your sessions have been terminated for security. Please login again.");
                    return Unauthorized(errorResponse);
                }

                //auth service 
                var response = ApiResponse<TokenDTO>.Ok(tokenResponse, "Token refreshed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, $"An error occurred during token refresh", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
