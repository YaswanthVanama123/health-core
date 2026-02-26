// ApplicationPreload.cs — Removed
//
// In IIS/ASP.NET 4.x, IProcessHostPreloadClient.Preload() was used to warm up
// the application before requests arrived, starting Hangfire via HangfireBootstrapper.
//
// In ASP.NET Core 8, application startup is handled entirely in Program.cs.
// Hangfire is started via IHostedService / IApplicationLifetime registered in Program.cs.
// IProcessHostPreloadClient does not exist in .NET 8.
//
// This file is intentionally left as a documentation stub.
