using WebApp.Components;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// 1. Radzen Services (muss vor RazorComponents registriert werden)
builder.Services.AddRadzenComponents();

// 2. Hybrid-Konfiguration (Server + WASM)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()    // Server-Rendering
    .AddInteractiveWebAssemblyComponents();  // WASM-Komponenten

// 3. HttpClient für API-Aufrufe (für WASM-Komponenten)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["API_URL"] ?? "https://localhost:7186")
});

// 4. CORS (falls WASM-Komponenten externe APIs aufrufen)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// 5. Middleware-Pipeline (UNBEDINGT NOTWENDIG)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Wichtig für Radzen (JS/CSS-Dateien)
app.UseAntiforgery();
app.UseCors("AllowAll");  // Falls CORS aktiviert

// 6. Hybrid-Routing (OHNE AddAdditionalAssemblies)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();  // Automatische Assembly-Erkennung

app.Run();