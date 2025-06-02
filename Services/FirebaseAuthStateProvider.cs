using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace Qalakar.Services;

public class FirebaseAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly FirebaseAuthService _authService;
    private bool _isInitialized = false;
    private AuthenticationState? _cachedState = null;

    public FirebaseAuthStateProvider(IJSRuntime jsRuntime, FirebaseAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            try
            {
                if (_jsRuntime != null)
                {
                    // Register this instance with JavaScript for auth state callbacks
                    var objRef = DotNetObjectReference.Create(this);
                    await _jsRuntime.InvokeVoidAsync("eval", "window.blazorAuthStateProvider = arguments[0]", objRef);
                    _isInitialized = true;
                }
            }
            catch
            {
                // Silent initialization failure
            }
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Initialize on first call
        if (!_isInitialized)
        {
            await InitializeAsync();
        }

        try
        {
            if (_authService == null) 
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            
            var user = await _authService.GetCurrentUserAsync();
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Uid),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("firebase_uid", user.Uid)
                };

                var identity = new ClaimsIdentity(claims, "firebase");
                var principal = new ClaimsPrincipal(identity);
                var authState = new AuthenticationState(principal);
                
                _cachedState = authState;
                return authState;
            }
        }
        catch
        {
            // Silent error handling
        }

        var unauthenticatedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        _cachedState = unauthenticatedState;
        return unauthenticatedState;
    }

    public void NotifyAuthenticationStateChanged()
    {
        try
        {
            _cachedState = null; // Clear cache to force fresh check
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch
        {
            // Silent error handling
        }
    }

    [JSInvokable]
    public void OnFirebaseAuthStateChanged()
    {
        try
        {
            NotifyAuthenticationStateChanged();
        }
        catch
        {
            // Silent error handling
        }
    }

    public void Dispose()
    {
        try
        {
            // Clean up if needed
        }
        catch
        {
            // Silent error handling
        }
    }
}