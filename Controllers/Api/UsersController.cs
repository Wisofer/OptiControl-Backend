using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUsuarioService _users;

    public UsersController(IUsuarioService users)
    {
        _users = users;
    }

    [HttpGet]
    [Authorize(Policy = "Administrador")]
    public IActionResult GetAll()
    {
        var list = _users.ObtenerTodos().Select(u => ToDto(u)).ToList();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Administrador")]
    public IActionResult GetById(int id)
    {
        var u = _users.ObtenerPorId(id);
        if (u == null) return NotFound();
        return Ok(ToDto(u));
    }

    [HttpPost]
    [Authorize(Policy = "Administrador")]
    public IActionResult Create([FromBody] UserCreateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.NombreUsuario) || string.IsNullOrWhiteSpace(dto.Contrasena))
            return BadRequest(new { error = "Usuario y contraseña son requeridos." });
        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario.Trim(),
            Contrasena = PasswordHelper.HashPassword(dto.Contrasena),
            NombreCompleto = dto.NombreCompleto?.Trim() ?? dto.NombreUsuario.Trim(),
            Rol = dto.Rol ?? SD.RolUsuario,
            Activo = true
        };
        if (!_users.Crear(usuario))
            return BadRequest(new { error = "El nombre de usuario ya existe." });
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, ToDto(usuario));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Administrador")]
    public IActionResult Update(int id, [FromBody] UserUpdateDto dto)
    {
        var u = _users.ObtenerPorId(id);
        if (u == null) return NotFound();
        if (dto == null) return BadRequest();

        u.NombreCompleto = dto.NombreCompleto ?? u.NombreCompleto;
        u.Rol = dto.Rol ?? u.Rol;
        u.Activo = dto.Estado == "Activo";
        if (!string.IsNullOrWhiteSpace(dto.NombreUsuario))
            u.NombreUsuario = dto.NombreUsuario;

        if (!string.IsNullOrWhiteSpace(dto.Contrasena))
            u.Contrasena = PasswordHelper.HashPassword(dto.Contrasena);

        if (!_users.Actualizar(u))
            return BadRequest(new { error = "Nombre de usuario ya existe." });
        return Ok(ToDto(u));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Administrador")]
    public IActionResult Delete(int id)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(currentUserId, out var currentId) && currentId == id)
            return BadRequest(new { error = "No puedes eliminar tu propio usuario." });

        var u = _users.ObtenerPorId(id);
        if (u == null)
            return NotFound(new { error = "Usuario no encontrado." });

        _users.Eliminar(id);
        return NoContent();
    }

    static object ToDto(OptiControl.Models.Entities.Usuario u)
    {
        return new
        {
            id = u.Id,
            usuario = u.NombreUsuario,
            nombreCompleto = u.NombreCompleto,
            rol = u.Rol,
            estado = u.Activo ? "Activo" : "Inactivo"
        };
    }
}

public class UserCreateDto
{
    public string NombreUsuario { get; set; } = "";
    public string Contrasena { get; set; } = "";
    public string? NombreCompleto { get; set; }
    public string? Rol { get; set; }
}

public class UserUpdateDto
{
    public string? NombreUsuario { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Rol { get; set; }
    public string? Estado { get; set; }
    public string? Contrasena { get; set; }
}
