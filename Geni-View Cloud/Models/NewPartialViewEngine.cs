// NewPartialViewEngine.cs — Stub for .NET 8 migration
//
// In ASP.NET MVC 5, NewPartialViewEngine extended RazorViewEngine to add
// "~/Views/PartialViews/{0}.cshtml" to the partial view search paths.
//
// In ASP.NET Core 8, this is handled via RazorViewEngineOptions in Program.cs:
//
//   builder.Services.Configure<RazorViewEngineOptions>(options =>
//   {
//       options.PartialViewLocationFormats.Add("~/Views/PartialViews/{0}.cshtml");
//   });
//
// This file is intentionally left as a documentation stub.
// RazorViewEngine is not available in Microsoft.AspNetCore.Mvc.
