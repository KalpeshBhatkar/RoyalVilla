using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<UserDTO?> RegisterAsync(RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (await IsEmailExistsAsync(registerationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"User with email '{registerationRequestDTO.Email}' already exists.");
                }

                ApplicationUser user = new()
                {
                    Email = registerationRequestDTO.Email,
                    Name = registerationRequestDTO.Name,
                    UserName = registerationRequestDTO.Email,
                    NormalizedEmail = registerationRequestDTO.Email.ToUpper(),
                    EmailConfirmed = true
                    //Password = registerationRequestDTO.Password,
                    //Role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role,
                    //CreatedDate = DateTime.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User registration failed: {errors}");
                }

                var role = string.IsNullOrEmpty(registerationRequestDTO.Role) ? "Customer" : registerationRequestDTO.Role;

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
                //await _db.Users.AddAsync(user);
                //await _db.SaveChangesAsync();

                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Role = role;

                return userDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred during user registration", ex);
            }
        }

        public async Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());


                if (user == null)
                {
                    return null; // user not found
                }

                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

                if (!isValid)
                {
                    return null; // invalid password
                }

                //generate TOKEN
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                //var roles = await _userManager.GetRolesAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jwtTokenId = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;

                //generate new refresh token
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshTokenAsync(user.Id, jwtTokenId, newRefreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new TokenDTO
                {
                    //UserDTO = _mapper.Map<UserDTO>(user),
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo,
                };

                //loginResponseDTO.UserDTO.Role = roles.FirstOrDefault() ?? "Customer";

                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred during user registration", ex);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            //return await _userManager.FindByEmailAsync(email);
            return await _db.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (await IsEmailExistsAsync(refreshTokenRequestDTO.RefreshToken))
                {
                    return null;
                }

                //Validate refresh Token
                var (isValid, userId, tokenFamilyId, tokenReused) = await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

                //Token Reuse Detected
                if (tokenReused)
                {
                    return null;
                }

                //Token is invalid or expired
                if (!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
                {
                    return null;
                }

                //get user
                var user = await _db.ApplicationUsers.FindAsync(userId);
                if(user == null)
                {
                    return null;
                }

                //revoke old refresh token
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

                //generate new access token and refresh Token
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                //var roles = await _userManager.GetRolesAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                //generate new refresh token
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new TokenDTO
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo,
                };

                //loginResponseDTO.UserDTO.Role = roles.FirstOrDefault() ?? "Customer";

                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred during token refresh", ex);
            }
        }
    }
}