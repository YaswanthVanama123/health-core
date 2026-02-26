// IdentityConfig.cs — ASP.NET Core Identity
//
// In ASP.NET Core Identity, UserManager and SignInManager are configured
// via AddIdentity<>() options in Program.cs, not via factory methods.
//
// Password policy, lockout settings, and two-factor providers are all
// registered in Program.cs:
//
//   builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//   {
//       options.Password.RequiredLength = 6;
//       options.Password.RequireNonAlphanumeric = true;
//       options.Password.RequireDigit = true;
//       options.Password.RequireLowercase = true;
//       options.Password.RequireUppercase = true;
//       options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//       options.Lockout.MaxFailedAccessAttempts = 3;
//       options.Lockout.AllowedForNewUsers = true;
//       options.User.RequireUniqueEmail = true;
//   })
//   .AddEntityFrameworkStores<ApplicationDbContext>()
//   .AddDefaultTokenProviders();
//
// Email sending is handled via IEmailSender registered in Program.cs.
// This file is intentionally left as a stub — no runtime code is needed here.
