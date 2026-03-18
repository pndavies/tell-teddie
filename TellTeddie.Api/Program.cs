using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using TellTeddie.Api.Services;
using TellTeddie.Infrastructure.BlobStorage;
using TellTeddie.Infrastructure.Repositories;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) Add core API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();

// Conditionally add services that depend on database/blob storage
var dbConnectionString = builder.Configuration.GetConnectionString("tellTeddieLocalDbConnectionString");
var blobConnectionString = builder.Configuration.GetConnectionString("petSwanBlobStorageConnectionString");

if (!string.IsNullOrEmpty(dbConnectionString) && !string.IsNullOrEmpty(blobConnectionString))
{
    builder.Services.AddHostedService<ExpiredPostService>();
    builder.Services.AddScoped<IPostFeedService, PostFeedService>();
    builder.Services.AddScoped<ITextPostService, TextPostService>();
    builder.Services.AddScoped<IAudioPostService, AudioPostService>();
    
    // 2) Repository registrations
    builder.Services.AddScoped<ITextPostRepository, TextPostRepository>();
    builder.Services.AddScoped<IAudioPostRepository, AudioPostRepository>();
    builder.Services.AddScoped<IPostRepository, PostRepository>();
    
    // 3) Database & blob connection factory
    builder.Services.AddSingleton<string>(sp => dbConnectionString);
    builder.Services.AddSingleton(sp =>
    {
        try
        {
            return new BlobServiceClient(blobConnectionString);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "The value for ConnectionStrings:petSwanBlobStorageConnectionString is not a valid Azure Blob Storage connection string. " +
                "If you are developing locally, set a real secret with 'dotnet user-secrets set \"ConnectionStrings:petSwanBlobStorageConnectionString\" \"<actual-connection-string>\" --project TellTeddie.Api'.",
                ex);
        }
    });
    
    // 4) Blob storage services
    builder.Services.AddScoped<IAzureAudioBlobService, AzureAudioBlobService>();
}

// CORS (allow calls from your Blazor app)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7222", "https://localhost:7223")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var problem = Results.Problem(
            title: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError,
            detail: app.Environment.IsDevelopment() ? feature?.Error.Message : ""
        );

        await problem.ExecuteAsync(context);
    });
});

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.Run();