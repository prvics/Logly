using Logly.Application.Auth.DTOs;
using Logly.Domain.Entities;

namespace Logly.Application.Auth.Services;

public interface IJwtTokenGenerator
{
    AuthResponse Create(AppUser user);
}