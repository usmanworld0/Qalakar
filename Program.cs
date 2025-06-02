using Microsoft.AspNetCore.Components.Authorization;
using Qalakar.Components;
using Qalakar.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirebaseAuthStateProvider>();
builder.Services.AddScoped<AuthStateService>(); // Add this line
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<FirebaseAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// Add GigService
builder.Services.AddScoped<GigService>();
builder.Services.AddScoped<StockPhotoService>(); // Add this line

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
