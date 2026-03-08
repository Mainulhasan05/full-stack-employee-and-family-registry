using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using family_registry_backend.DTOs;
using family_registry_backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace family_registry_backend.Services;

public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return null;

        return await GenerateToken(user);
    }

    public async Task<(AuthResponseDto? Response, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (null, result.Errors.Select(e => e.Description));

        // Validate role
        var role = dto.Role == "Admin" ? "Admin" : "Viewer";
        await _userManager.AddToRoleAsync(user, role);

        var response = await GenerateToken(user);
        return (response, Enumerable.Empty<string>());
    }

    private async Task<AuthResponseDto> GenerateToken(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Viewer";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Role, role),
            new("FullName", user.FullName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpireHours"] ?? "24"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserName = user.UserName!,
            FullName = user.FullName,
            Role = role,
            Expiration = expiration
        };
    }
}
