using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace sistemasconsultas.Services;

public class UsuarioAtual(AuthenticationStateProvider authProvider)
{
    public async Task<string> ObterIdAsync()
    {
        var state = await authProvider.GetAuthenticationStateAsync();
        var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("Usuário não autenticado.");

        return userId;
    }
}