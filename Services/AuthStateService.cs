using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using Timer = System.Threading.Timer;
using Qalakar.Services;

namespace Qalakar.Services;

public class AuthStateService : IDisposable
{
    private readonly FirebaseAuthService _authService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IJSRuntime _jsRuntime;
    private Timer? _authCheckTimer;
    private FirebaseUser? _currentUser;
    private bool _isAuthenticated = false;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    public event Action? OnAuthStateChanged;

    public bool IsAuthenticated => _isAuthenticated;
    public FirebaseUser? CurrentUser => _currentUser;
    public bool IsInitialized => _isInitialized;

    public AuthStateService(FirebaseAuthService authService, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
    {
        _authService = authService;
        _authStateProvider = authStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        await _initSemaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            // Check current user state from Firebase
            await CheckCurrentUser();
            _isInitialized = true;

            _authCheckTimer = new Timer(async _ =>
            {
                try
                {
                    await CheckCurrentUser();
                }
                catch
                {
                    // Silent error handling
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    public async Task<bool> CheckCurrentUser()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null && !string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Uid))
            {
                SetAuthState(true, user);
                return true;
            }

            SetAuthState(false, null);
            return false;
        }
        catch
        {
            SetAuthState(false, null);
            return false;
        }
    }

    public void SetAuthState(bool isAuthenticated, FirebaseUser? user)
    {
        var stateChanged = _isAuthenticated != isAuthenticated ||
                          (_currentUser?.Uid != user?.Uid);

        _isAuthenticated = isAuthenticated;
        _currentUser = user;

        if (stateChanged)
        {
            OnAuthStateChanged?.Invoke();
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _authService.SignOutAsync();
        }
        catch { }
        finally
        {
            SetAuthState(false, null);
        }
    }

    [JSInvokable]
    public async Task OnFirebaseAuthStateChanged()
    {
        await CheckCurrentUser();
    }

    private async Task CheckAuthenticationState()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            var wasAuthenticated = _currentUser != null;
            var currentEmail = _currentUser?.Email;

            _currentUser = user;

            if ((wasAuthenticated != IsAuthenticated) || (currentEmail != user?.Email))
            {
                OnAuthStateChanged?.Invoke();
            }
        }
        catch
        {
            var wasAuthenticated = _currentUser != null;
            _currentUser = null;

            if (wasAuthenticated)
            {
                OnAuthStateChanged?.Invoke();
            }
        }
    }

    public void Dispose()
    {
        _authCheckTimer?.Dispose();
    }
}
