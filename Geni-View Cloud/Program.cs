// Program.cs - ASP.NET Core 8 Entry Point
// This file replaces Global.asax.cs, Startup.cs (OWIN), and Startup.Auth.cs
// It will be expanded in each subsequent migration phase.
//
// Phase 1: Minimal stub to confirm project structure compiles.
// Phase 3: Full middleware pipeline (EF Core, Identity, Hangfire, SignalR, MQTT) added here.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// --- Services will be registered here in Phase 3 ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Middleware pipeline will be configured here in Phase 3 ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route - Dashboard is the home page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
