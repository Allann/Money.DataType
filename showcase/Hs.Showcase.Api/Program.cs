using Hs.Showcase.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// The React app, built into wwwroot by `npm run build` in showcase/web.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapShowcase();

// Client-side routes (/tour/core, ...) load the app; unknown /api routes stay 404s.
app.MapFallback("/api/{**path}", () => TypedResults.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
