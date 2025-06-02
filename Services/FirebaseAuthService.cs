using Microsoft.JSInterop;
using System.Text.Json;

namespace Qalakar.Services;

public class FirebaseAuthService
{
    private readonly IJSRuntime _jsRuntime;

    public FirebaseAuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<AuthResult> SignUpAsync(string email, string password)
    {
        try
        {
            // Wait for JS runtime to be ready and Firebase to load
            await Task.Delay(500);
            
            // Check if firebaseAuth is available
            var isAvailable = await _jsRuntime.InvokeAsync<bool>("eval", "typeof window.firebaseAuth !== 'undefined'");
            if (!isAvailable)
            {
                return new AuthResult { Success = false, Message = "Firebase is not loaded. Please refresh the page and try again." };
            }
            
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseAuth.signUp", email, password);
            return new AuthResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (JSException jsEx)
        {
            return new AuthResult { Success = false, Message = $"JavaScript error: {jsEx.Message}" };
        }
        catch (InvalidOperationException)
        {
            return new AuthResult { Success = false, Message = "Firebase is not initialized. Please refresh the page and try again." };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { Success = false, Message = "Request timed out. Please try again." };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            // Reduce delay and add timeout
            var timeoutTask = Task.Delay(10000); // 10 second timeout
            var isAvailable = await _jsRuntime.InvokeAsync<bool>("eval", "typeof window.firebaseAuth !== 'undefined'");
            
            if (!isAvailable)
            {
                return new AuthResult { Success = false, Message = "Firebase is not loaded. Please refresh the page and try again." };
            }
            
            var signInTask = _jsRuntime.InvokeAsync<JsonElement>("firebaseAuth.signIn", email, password).AsTask();
            var completedTask = await Task.WhenAny(signInTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                return new AuthResult { Success = false, Message = "Sign in timed out. Please try again." };
            }
            
            var result = await signInTask;
            var authResult = new AuthResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };

            if (authResult.Success && result.TryGetProperty("user", out var userElement))
            {
                authResult.User = new FirebaseUser
                {
                    Email = userElement.GetProperty("email").GetString() ?? "",
                    Uid = userElement.GetProperty("uid").GetString() ?? ""
                };
            }

            return authResult;
        }
        catch (JSException jsEx)
        {
            return new AuthResult { Success = false, Message = $"JavaScript error: {jsEx.Message}" };
        }
        catch (InvalidOperationException)
        {
            return new AuthResult { Success = false, Message = "Firebase is not initialized. Please refresh the page and try again." };
        }
        catch (TaskCanceledException)
        {
            return new AuthResult { Success = false, Message = "Request timed out. Please try again." };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<AuthResult> SignOutAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseAuth.signOut");
            return new AuthResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<AuthResult> ResendVerificationAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseAuth.resendVerification");
            return new AuthResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<FirebaseUser?> GetCurrentUserAsync()
    {
        try
        {
            // Check if JS runtime is available
            if (_jsRuntime == null) return null;
            
            // First check if Firebase is loaded
            var isFirebaseLoaded = false;
            try
            {
                isFirebaseLoaded = await _jsRuntime.InvokeAsync<bool>("eval", 
                    "typeof window.firebaseAuth !== 'undefined' && window.firebaseAuth !== null");
            }
            catch
            {
                return null;
            }
            
            if (!isFirebaseLoaded)
            {
                return null;
            }
            
            // Use a longer timeout for initial auth checks
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(3000));
            var result = await _jsRuntime.InvokeAsync<JsonElement?>("firebaseAuth.getCurrentUser", cts.Token);
            
            if (result.HasValue && result.Value.ValueKind != JsonValueKind.Null)
            {
                var email = result.Value.GetProperty("email").GetString() ?? "";
                var uid = result.Value.GetProperty("uid").GetString() ?? "";
                
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(uid))
                {
                    return new FirebaseUser
                    {
                        Email = email,
                        Uid = uid
                    };
                }
            }
            return null;
        }
        catch (TaskCanceledException)
        {
            // Timeout occurred
            return null;
        }
        catch (JSException)
        {
            // JavaScript error - Firebase might not be ready
            return null;
        }
        catch
        {
            return null;
        }
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public FirebaseUser? User { get; set; }
}

public class FirebaseUser
{
    public string Email { get; set; } = "";
    public string Uid { get; set; } = "";
}
