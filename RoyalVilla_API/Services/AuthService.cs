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

        public AuthService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
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
                var token = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);

                TokenDTO tokenDTO = new TokenDTO
                {
                    //UserDTO = _mapper.Map<UserDTO>(user),
                    AccessToken = token
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

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);
            var roles = await _userManager.GetRolesAsync(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}