using RoyalVilla.DTO;

namespace RoyalVilla_API.Services.IServices
{
    public interface IAuthService
    {
        Task<UserDTO?> RegisterAsync(RegisterationRequestDTO registerationRequestDTO);
        Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO registerationRequestDTO);
        Task<bool> IsEmailExistsAsync(string email);
    }
}
