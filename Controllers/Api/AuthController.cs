using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Services.IServices;
using System.Security.Claims;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.NombreUsuario) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest(new { error = "Usuario y contraseña son requeridos." });
        }

        var usuario = _authService.ValidarUsuario(request.NombreUsuario, request.Contrasena);
        if (usuario == null)
        {
            return Unauthorized(new { error = "Usuario o contraseña incorrectos." });
        }

        var token = _jwtTokenService.GenerateToken(usuario);

        return Ok(new
        {
            token,
            user = new
            {
                id = usuario.Id,
                usuario = usuario.NombreUsuario,
                nombreCompleto = usuario.NombreCompleto,
                rol = usuario.Rol,
                estado = usuario.Activo ? "Activo" : "Inactivo"
            }
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuario = User.FindFirst(ClaimTypes.Name)?.Value;
        var nombreCompleto = User.FindFirst("NombreCompleto")?.Value;
        var rol = User.FindFirst("Rol")?.Value;
        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        return Ok(new
        {
            id = int.Parse(id),
            usuario,
            nombreCompleto,
            rol,
            estado = "Activo"
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Con JWT no hay invalidación en servidor: el frontend borra el token.
        return Ok(new { success = true });
    }
}

public class LoginRequest
{
    public string NombreUsuario { get; set; } = "";
    public string Contrasena { get; set; } = "";
}
