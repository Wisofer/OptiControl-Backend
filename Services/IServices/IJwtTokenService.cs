using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IJwtTokenService
{
    string GenerateToken(Usuario usuario);
}
