// Program.cs — ASP.NET Core 8 entry point
// Replaces: Global.asax.cs, Startup.cs (OWIN), Startup.Auth.cs, HangfireBootstrapper.cs, WebHost.cs
//
// Phase 3: Full middleware pipeline wired up.
// Phase 4: Identity migrations + seed will be triggered from the EnsureDatabaseAsync call below.
// Phase 6: MQTTBackgroundService registered here.
// Phase 8: WebOptimizer bundles registered here.

using GeniView.Cloud.Common;
using GeniView.Cloud.Hubs;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Threading.Tasks;
using WebOptimizer;

// ── Bootstrap NLog early so startup errors are captured ────────────────────
var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()
                       .GetCurrentClassLogger();
logger.Info("Application starting.");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // ── NLog as the logging provider ────────────────────────────────────────
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // ── MVC + Razor views ───────────────────────────────────────────────────
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(opts =>
        {
            // Preserve PascalCase JSON serialization to match the old ASP.NET MVC
            // Newtonsoft.Json default. All existing JavaScript reads property names
            // in PascalCase (e.g. data.PowerModulesCount, data.HighSoCCount).
            opts.JsonSerializerOptions.PropertyNamingPolicy = null;
        })
        .AddRazorOptions(opts =>
        {
            // Preserve the legacy partial view location used by NewPartialViewEngine.
            opts.ViewLocationFormats.Add("/Views/PartialViews/{0}.cshtml");
            opts.AreaViewLocationFormats.Add("/Areas/{2}/Views/PartialViews/{0}.cshtml");
        });

    // ── EF Core — Identity DB ───────────────────────────────────────────────
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseSqlServer(
            configuration.GetConnectionString("GeniViewCloudIdentityRepository"),
            sql => sql.EnableRetryOnFailure()));

    // ── EF Core — Application data DB ──────────────────────────────────────
    builder.Services.AddDbContext<GeniViewCloudDataRepository>(opts =>
        opts.UseSqlServer(
            configuration.GetConnectionString("GeniViewCloudDataRepository"),
            sql => sql.EnableRetryOnFailure()));

    // ── ASP.NET Core Identity ───────────────────────────────────────────────
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
    {
        // Password policy (matches original IdentityConfig.cs)
        opts.Password.RequireDigit           = false;
        opts.Password.RequireLowercase       = false;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase       = false;
        opts.Password.RequiredLength         = 6;

        // Lockout policy
        opts.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(
            configuration.GetValue<int>("AppSettings:UserLockoutTimeInMinutes", 5));
        opts.Lockout.MaxFailedAccessAttempts = 5;
        opts.Lockout.AllowedForNewUsers      = true;

        // User settings
        opts.User.RequireUniqueEmail = true;

        // Sign-in: email confirmation not required during initial migration
        opts.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Cookie config — matches original FormsAuthentication behaviour
    builder.Services.ConfigureApplicationCookie(opts =>
    {
        opts.LoginPath        = "/Account/Login";
        opts.LogoutPath       = "/Account/LogOff";
        opts.AccessDeniedPath = "/Account/Login";
        opts.ExpireTimeSpan   = TimeSpan.FromDays(14);
        opts.SlidingExpiration = true;
    });

    // ── Session ─────────────────────────────────────────────────────────────
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opts =>
    {
        opts.IdleTimeout        = TimeSpan.FromMinutes(30);
        opts.Cookie.HttpOnly    = true;
        opts.Cookie.IsEssential = true;
    });

    // ── IHttpContextAccessor (used by SessionHelper) ────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── In-memory cache (used by MemCacheHelper) ────────────────────────────
    builder.Services.AddMemoryCache();

    // ── SignalR ──────────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── WebOptimizer (Phase 8) — CSS + JS bundles ────────────────────────────
    builder.Services.AddWebOptimizer(pipeline =>
    {
        // Main layout CSS bundle
        pipeline.AddCssBundle("/bundles/main.css",
            "css/main.css",
            "css/global-nav.css",
            "css/dataTables.min.css",
            "css/alertify.min.css");

        // Admin layout CSS bundle
        pipeline.AddCssBundle("/bundles/admin.css",
            "css/bootstrap-colorpicker.min.css",
            "css/alertify.min.css",
            "css/dataTables.min.css");

        // Core JS bundle (jQuery + Bootstrap + common utilities)
        pipeline.AddJavaScriptBundle("/bundles/lib.js",
            "js/jquery-3.7.1.min.js",
            "js/bootstrap.min.js",
            "js/jquery.matchHeight.js",
            "js/alertify.min.js");

        // App JS bundle (shared across all pages)
        pipeline.AddJavaScriptBundle("/bundles/app.js",
            "js/Custom Scripts/global-filters.js",
            "js/Custom Scripts/datatables.min.js",
            "js/Custom Scripts/notification.js",
            "js/Custom Scripts/app.js");

        // Admin JS bundle
        pipeline.AddJavaScriptBundle("/bundles/admin-app.js",
            "js/Custom Scripts/datatables.min.js",
            "js/jquery.matchHeight.js",
            "js/alertify.min.js",
            "js/Custom Scripts/bootstrap-colorpicker.min.js",
            "js/Custom Scripts/notification.js",
            "js/Custom Scripts/app.js");
    });

    // ── Hangfire — use SQL Server storage ───────────────────────────────────
    builder.Services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            configuration.GetConnectionString("GeniViewCloudHangfireRepository"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                QueuePollInterval            = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks           = true
            }));
    builder.Services.AddHangfireServer();

    // ── MQTT singleton + BackgroundService (Phase 6) ────────────────────────
    builder.Services.AddSingleton<MQTTHelper>(sp =>
        new MQTTHelper(sp.GetRequiredService<IConfiguration>()));
    builder.Services.AddHostedService<MQTTBackgroundService>();

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────

    // ── Initialise static helpers that need IConfiguration ──────────────────
    GlobalSettings.Initialize(configuration);

    // ── Initialise SessionHelper with IHttpContextAccessor ──────────────────
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    SessionHelper.Configure(httpContextAccessor);

    // ── Set Global._serverPath for MailHelper / OTA file paths ──────────────
    Global._serverPath = app.Environment.ContentRootPath;

    // ── Apply EF Core migrations and seed the database ──────────────────────
    await EnsureDatabaseAsync(app);

    // ── Exception handling ───────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // ── Middleware pipeline (ORDER MATTERS) ──────────────────────────────────
    app.UseHttpsRedirection();
    app.UseWebOptimizer();      // must be before UseStaticFiles
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();           // must be before Authentication
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Hangfire dashboard (/hangfire) — restricted to Application Admin ─────
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangFireAuthorizationFilter() }
    });

    // ── SignalR hub ──────────────────────────────────────────────────────────
    app.MapHub<NotificationHub>("/notificationHub");

    // ── MVC routes ───────────────────────────────────────────────────────────
    // Admin area route must come before the default route
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    // ── Hangfire recurring jobs (Phase 6) ────────────────────────────────────
    // Hangfire's built-in AspNetCoreJobActivator resolves LogApiController
    // from a DI scope per job execution — no direct instantiation needed.
    new HFScheduler().Setting();

    logger.Info("Application started successfully.");
    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}

// ── Helper: apply EF Core migrations + seed roles/admin user ────────────────
static async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var log = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<WebApplication>>();

    try
    {
        var identityDb = services.GetRequiredService<ApplicationDbContext>();

        // Upgrade 4.8 Identity schema to Core Identity schema before running migrations
        await UpgradeIdentitySchemaAsync(identityDb, log);

        try { await identityDb.Database.MigrateAsync(); }
        catch { await identityDb.Database.EnsureCreatedAsync(); }

        // Application data DB
        var dataDb = services.GetRequiredService<GeniViewCloudDataRepository>();
        try { await dataDb.Database.MigrateAsync(); }
        catch { await dataDb.Database.EnsureCreatedAsync(); }

        // Seed roles and default admin user
        await SeedAsync(services, log);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Database migration/seed failed. The app will still start.");
    }
}

// Upgrades a restored .NET 4.8 ASP.NET Identity 2.x database to ASP.NET Core Identity 8 schema.
// All statements are safe to run on every startup — idempotent IF NOT EXISTS / IF EXISTS guards.
//
// Split into 4 separate ExecuteSqlRawAsync calls because SQL Server compiles an entire batch
// before executing it; an UPDATE referencing a newly-added column in the same batch fails at
// compile time even if the ALTER TABLE precedes it.
//
// FK constraints are intentionally omitted from the new tables: a restored 4.8 backup has
// AspNetRoles.Id as nvarchar(128) while EF Core 8 defaults to nvarchar(450), and SQL Server
// rejects FK columns whose lengths differ from the referenced PK column.
static async Task UpgradeIdentitySchemaAsync(ApplicationDbContext db, Microsoft.Extensions.Logging.ILogger log)
{
    try
    {
        // ── Batch 1: DDL — add columns that Core Identity requires ───────────────
        // Outer IF EXISTS ensures we only ALTER tables that already exist
        // (i.e. a restored 4.8 backup). Fresh databases have no tables yet and
        // MigrateAsync handles their creation from scratch.
        await db.Database.ExecuteSqlRawAsync("""
            IF EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetRoles' AND type='U')
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetRoles') AND name='NormalizedName')
                    ALTER TABLE AspNetRoles ADD NormalizedName nvarchar(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetRoles') AND name='ConcurrencyStamp')
                    ALTER TABLE AspNetRoles ADD ConcurrencyStamp nvarchar(max) NULL;
            END
            IF EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetUsers' AND type='U')
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetUsers') AND name='NormalizedUserName')
                    ALTER TABLE AspNetUsers ADD NormalizedUserName nvarchar(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetUsers') AND name='NormalizedEmail')
                    ALTER TABLE AspNetUsers ADD NormalizedEmail nvarchar(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetUsers') AND name='ConcurrencyStamp')
                    ALTER TABLE AspNetUsers ADD ConcurrencyStamp nvarchar(max) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('AspNetUsers') AND name='LockoutEnd')
                    ALTER TABLE AspNetUsers ADD LockoutEnd datetimeoffset NULL;
            END
            """);

        // ── Batch 2: DML — populate normalized columns ───────────────────────────
        // Must be a separate batch from Batch 1: SQL Server compiles the whole batch
        // before executing it, so an UPDATE referencing a just-added column fails.
        // sp_executesql defers column resolution to runtime.
        // IF EXISTS guards make this a no-op on fresh databases.
        await db.Database.ExecuteSqlRawAsync("""
            IF EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetRoles' AND type='U')
                EXEC sp_executesql N'UPDATE AspNetRoles SET NormalizedName = UPPER(Name) WHERE NormalizedName IS NULL';
            IF EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetUsers' AND type='U')
            BEGIN
                EXEC sp_executesql N'UPDATE AspNetUsers SET NormalizedUserName = UPPER(UserName) WHERE NormalizedUserName IS NULL';
                EXEC sp_executesql N'UPDATE AspNetUsers SET NormalizedEmail = UPPER(Email) WHERE NormalizedEmail IS NULL';
            END
            """);

        // ── Batch 3: Create tables that did not exist in Identity 2.x ────────────
        // No FK constraints: a 4.8 backup has Id columns as nvarchar(128) while
        // Core Identity uses nvarchar(450); SQL Server rejects FK column length mismatches.
        // EF Core queries work correctly without FK constraints on the database side.
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetRoleClaims' AND type='U')
               AND EXISTS  (SELECT 1 FROM sys.objects WHERE name='AspNetRoles'     AND type='U')
            BEGIN
                CREATE TABLE AspNetRoleClaims (
                    Id        int          IDENTITY(1,1) NOT NULL,
                    RoleId    nvarchar(128) NOT NULL,
                    ClaimType nvarchar(max) NULL,
                    ClaimValue nvarchar(max) NULL,
                    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id)
                );
                CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims(RoleId);
            END
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetUserTokens' AND type='U')
               AND EXISTS  (SELECT 1 FROM sys.objects WHERE name='AspNetUsers'     AND type='U')
            BEGIN
                CREATE TABLE AspNetUserTokens (
                    UserId        nvarchar(128) NOT NULL,
                    LoginProvider nvarchar(128) NOT NULL,
                    Name          nvarchar(128) NOT NULL,
                    Value         nvarchar(max) NULL,
                    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name)
                );
            END
            """);

        // ── Batch 4: Mark the EF Core initial migration as applied ───────────────
        // Only do this once both new tables exist, so MigrateAsync does not try to
        // recreate tables that were just built above (or already existed).
        await db.Database.ExecuteSqlRawAsync("""
            IF EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetRoleClaims' AND type='U')
               AND EXISTS (SELECT 1 FROM sys.objects WHERE name='AspNetUserTokens' AND type='U')
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name='__EFMigrationsHistory')
                    CREATE TABLE __EFMigrationsHistory (
                        MigrationId    nvarchar(150) NOT NULL,
                        ProductVersion nvarchar(32)  NOT NULL,
                        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
                    );
                IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId='20260226145844_InitialIdentity')
                    INSERT INTO __EFMigrationsHistory VALUES ('20260226145844_InitialIdentity', '8.0.0');
            END
            """);

        log.LogInformation("Identity schema upgrade completed.");
    }
    catch (Exception ex)
    {
        log.LogWarning(ex, "Identity schema upgrade skipped (database may not exist yet).");
    }
}



static async Task SeedAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger log)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed roles
    string[] roles =
    {
        "Application Admin",
        "Application User",
        "Community Admin",
        "Community Group Admin",
        "Community User"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            log.LogInformation("Created role: {Role}", role);
        }
    }

    // Seed default Application Admin user
    const string adminEmail    = "admin@bytec.com";
    const string adminPassword = "Admin@123!";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName              = adminEmail,
            Email                 = adminEmail,
            FullName              = "Application Admin",
            EmailConfirmed        = true,
            IsNotificationEnable  = true,
            LockoutEnabled        = false
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Application Admin");
            log.LogInformation("Seeded default admin user: {Email}", adminEmail);
        }
        else
        {
            foreach (var e in result.Errors)
                log.LogWarning("Seed admin error: {Code} — {Description}", e.Code, e.Description);
        }
    }
}
