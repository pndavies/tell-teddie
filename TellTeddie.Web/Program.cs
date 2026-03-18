using TellTeddie.Web.Services;
using TellTeddie.Web;

var builder = WebApplication.CreateBuilder(args);

// 1) Add Razor Components (server-side) and HTTP client for your API
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationInsightsTelemetry();

// Register Error State Service
builder.Services.AddScoped<IErrorStateService, ErrorStateService>();

builder.Services.AddScoped<ApiErrorRedirectHandler>();

var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5029";
builder.Services.AddHttpClient("TellTeddieApi", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
}).AddHttpMessageHandler<ApiErrorRedirectHandler>();

// Register Coming Soon Service
builder.Services.AddScoped<IComingSoonService, ComingSoonService>();

// Only register API-dependent services if we have a valid API connection
if (!string.IsNullOrEmpty(apiBaseAddress) && apiBaseAddress != "coming-soon")
{
    builder.Services.AddScoped<ITextPostService, TextPostService>();
    builder.Services.AddScoped<IAudioPostService, AudioPostService>();
    builder.Services.AddScoped<IPostService, PostService>();
}

var app = builder.Build();

// 2) Error handling & security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error500");
    app.UseHsts();
}

// 3) Static files, routing, and Blazor endpoints
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4) Add antiforgery middleware
app.UseAntiforgery();

// 5) Blazor endpoint
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
