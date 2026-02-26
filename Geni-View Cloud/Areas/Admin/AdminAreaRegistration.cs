// AdminAreaRegistration.cs — Removed
//
// In ASP.NET MVC 5, AreaRegistration.RegisterArea() was used to register the Admin area.
// In ASP.NET Core 8, areas are registered by:
//   1. Adding [Area("Admin")] attribute to all admin controllers
//   2. Adding the area route in Program.cs:
//        app.MapControllerRoute("admin", "Admin/{controller=Home}/{action=Index}/{id?}",
//            defaults: new {}, constraints: new { area = "Admin" });
//   OR using the standard areas convention:
//        app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
//
// [Area("Admin")] has been added to admin controllers in Phase 5.
// This file is intentionally left as a documentation stub.
