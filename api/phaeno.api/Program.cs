using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using phaeno.api.Configuration;
using phaeno.api.Features.WebOps.DataAccess;
using phaeno.api.Features.WebOps.Services;
using phaeno.api.Infrastructure.Api;
using phaeno.api.Infrastructure.ChronJobs.Jobs;
using phaeno.api.Infrastructure.Db;
using phaeno.api.Infrastructure.NotificationServices;
using phaeno.api.Infrastructure.NotificationServices.Email;
using phaeno.api.Infrastructure.RecatchaServices;
using phaeno.api.Infrastructure.WebcrawlerServices;
using phaeno.api.Infrastructure.WebSearchServices;
using phaeno.api.Middleware;
using Quartz;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => new
            {
                field = kvp.Key,
                messages = kvp.Value!.Errors.Select(e =>
                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Invalid value."
                        : e.ErrorMessage)
            })
            .ToList();

        var response = ApiResponse<object>.Fail(
            new ApiError(
                type: "invalid_request",
                code: "validation_error",
                message: "One or more validation errors occurred.",
                details: errors),
            ApiMetaFactory.Create(context.HttpContext));

        return new BadRequestObjectResult(response);
    };
});

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add<ApiResponseEnvelopeFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Website", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.DocInclusionPredicate((docName, apiDesc) =>
        string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath);
});
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.Configure<GoogleAuthSettings>(
    builder.Configuration.GetSection("GoogleAuthSettings"));
builder.Services.Configure<WebCrawlerSettings>(
    builder.Configuration.GetSection("WebCrawlerSettings"));
builder.Services.Configure<WebSearchSettings>(
    builder.Configuration.GetSection("WebSearchSettings"));
builder.Services.Configure<EmailServiceSettings>(
    builder.Configuration.GetSection("EmailServiceSettings"));
builder.Services.Configure<ChronJobScheduleOptions>(
    builder.Configuration.GetSection("ChronJobs"));

#if DEBUG
const string connectionStringSource = "Development";
#else
var connectionStringSource = builder.Environment.IsDevelopment() ? "Development" : "Production";
#endif

var connectionString = builder.Configuration.GetConnectionString(connectionStringSource);

builder.Services.AddDbContext<PseqDatabase>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorCodesToAdd: null)
            .CommandTimeout(30);
    });
});

builder.Services.AddScoped<WebOpsQueries>();
builder.Services.AddScoped<WebOpsCommands>();
builder.Services.AddScoped<WebOpsService>();
builder.Services.AddScoped<RecaptchaService>();
builder.Services.AddScoped<IWebSearchService, WebSearchService>();
builder.Services.AddHttpClient<IWebcrawlerService, WebcrawlerService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<IEmailNotifier, MailgunEmailNotifier>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<EmailServiceSettings>>().Value;
    client.BaseAddress = new Uri(settings.Url);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"api:{settings.ApiKey}")));
});

builder.Services.AddQuartz(q =>
{
    var schedules = builder.Configuration
        .GetSection("ChronJobs")
        .Get<ChronJobScheduleOptions>()
        ?? throw new InvalidOperationException("ChronJobs config missing.");

    var indexJobKey = new JobKey(nameof(IndexWebsiteJob));

    q.AddJob<IndexWebsiteJob>(opts =>
        opts.WithIdentity(indexJobKey)
            .StoreDurably());

    q.AddTrigger(t =>
    {
        t.ForJob(indexJobKey)
            .WithIdentity($"{nameof(IndexWebsiteJob)}Trigger")
            .StartNow()
            .WithSimpleSchedule(x =>
                x.WithIntervalInHours(Math.Max(1, schedules.IndexWebsite.IntervalHours))
                    .RepeatForever());
    });
});

builder.Services.AddQuartzHostedService(o =>
{
    o.WaitForJobsToComplete = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((doc, req) =>
        {
            doc.Servers = new List<OpenApiServer>
            {
                new() { Url = $"{req.Scheme}://{req.Host.Value}/api" }
            };
        });
    });

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.MapScalarApiReference("/documentation", options =>
    {
        options.WithTitle("Phaeno-Portal API")
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");

        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.AddDocument(
                desc.GroupName,
                desc.GroupName.ToUpperInvariant());
        }
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
var publicFilesPath = Path.Combine(app.Environment.ContentRootPath, "__DOCUMENTS/public");
Directory.CreateDirectory(publicFilesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicFilesPath),
    RequestPath = "/public"
});
app.UseRouting();
app.UseCors("Website");

app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api"),
    apiApp =>
    {
        apiApp.UseMiddleware<ApiExceptionMiddleware>();
    });

var api = app.MapGroup("/api");
api.MapControllers();

app.Run();

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        this.provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "Phaeno-Portal API",
                Version = desc.ApiVersion.ToString(),
                Description = desc.IsDeprecated
                    ? "This API version has been deprecated."
                    : null
            });
        }
    }
}
