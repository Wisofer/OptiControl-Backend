using Microsoft.EntityFrameworkCore;
using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Services;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public UsuarioService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    public List<Usuario> ObtenerTodos()
    {
        return _context.Usuarios
            .OrderBy(u => u.NombreUsuario)
            .ToList();
    }

    public Usuario? ObtenerPorId(int id)
    {
        return _context.Usuarios.Find(id);
    }

    public Usuario? ObtenerPorNombreUsuario(string nombreUsuario)
    {
        return _context.Usuarios
            .FirstOrDefault(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower());
    }

    public bool Crear(Usuario usuario)
    {
        if (ExisteNombreUsuario(usuario.NombreUsuario))
        {
            return false;
        }

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeUser, $"Usuario creado: {usuario.NombreUsuario} ({usuario.NombreCompleto})", usuario.Id.ToString(), null);
        return true;
    }

    public bool Actualizar(Usuario usuario)
    {
        var usuarioExistente = _context.Usuarios.Find(usuario.Id);
        if (usuarioExistente == null)
        {
            return false;
        }

        if (ExisteNombreUsuario(usuario.NombreUsuario, usuario.Id))
        {
            return false;
        }

        usuarioExistente.NombreUsuario = usuario.NombreUsuario;
        usuarioExistente.NombreCompleto = usuario.NombreCompleto;
        usuarioExistente.Rol = usuario.Rol;
        usuarioExistente.Activo = usuario.Activo;

        // Solo actualizar contraseña si se proporcionó una nueva
        if (!string.IsNullOrWhiteSpace(usuario.Contrasena))
        {
            usuarioExistente.Contrasena = usuario.Contrasena;
        }

        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeUser, $"Usuario actualizado: {usuarioExistente.NombreUsuario} ({usuarioExistente.NombreCompleto})", usuarioExistente.Id.ToString(), null);
        return true;
    }

    public bool Eliminar(int id)
    {
        var usuario = _context.Usuarios.Find(id);
        if (usuario == null)
        {
            return false;
        }
        var nombre = usuario.NombreUsuario;
        var nombreCompleto = usuario.NombreCompleto;
        _context.Usuarios.Remove(usuario);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeUser, $"Usuario eliminado: {nombre} ({nombreCompleto})", id.ToString(), null);
        return true;
    }

    public bool ExisteNombreUsuario(string nombreUsuario, int? idExcluir = null)
    {
        var query = _context.Usuarios
            .Where(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower());

        if (idExcluir.HasValue)
        {
            query = query.Where(u => u.Id != idExcluir.Value);
        }

        return query.Any();
    }
}

