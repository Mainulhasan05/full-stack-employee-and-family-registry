using family_registry_backend.DTOs;
using family_registry_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace family_registry_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (response, errors) = await _authService.RegisterAsync(dto);
        if (response == null)
            return BadRequest(new { message = "Registration failed.", errors });

        return Ok(response);
    }
}
