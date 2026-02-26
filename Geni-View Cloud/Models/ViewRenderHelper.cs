// ViewRenderHelper.cs — ASP.NET Core stub
//
// The MVC 5 ViewRenderer used System.Web.Mvc ControllerContext / ViewEngines.Engines
// to render Razor views to strings for email templates.
//
// In ASP.NET Core 8, the equivalent is IRazorViewEngine + ICompositeViewEngine.
// Full implementation is deferred to Phase 5 (Controllers + Views migration).
//
// Pattern to implement in Phase 5:
//
//   public class ViewRenderService
//   {
//       private readonly IRazorViewEngine _viewEngine;
//       private readonly ITempDataProvider _tempDataProvider;
//       private readonly IServiceProvider _serviceProvider;
//
//       public ViewRenderService(IRazorViewEngine viewEngine,
//           ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
//       {
//           _viewEngine = viewEngine;
//           _tempDataProvider = tempDataProvider;
//           _serviceProvider = serviceProvider;
//       }
//
//       public async Task<string> RenderToStringAsync(string viewName, object model)
//       {
//           var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
//           var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
//           using var sw = new StringWriter();
//           var viewResult = _viewEngine.FindView(actionContext, viewName, false);
//           if (!viewResult.Success) throw new InvalidOperationException($"View '{viewName}' not found.");
//           var viewContext = new ViewContext(actionContext, viewResult.View,
//               new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
//               new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
//               sw, new HtmlHelperOptions());
//           await viewResult.View.RenderAsync(viewContext);
//           return sw.ToString();
//       }
//   }
//
// Register in Program.cs: builder.Services.AddTransient<ViewRenderService>();
