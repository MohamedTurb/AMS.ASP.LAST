using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using AssistanceManagementSystem.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var dotNetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
if (string.IsNullOrWhiteSpace(aspNetCoreEnvironment) && string.IsNullOrWhiteSpace(dotNetEnvironment))
{
    if (string.IsNullOrWhiteSpace(builder.Environment.EnvironmentName))
    {
        builder.Environment.EnvironmentName = Microsoft.Extensions.Hosting.Environments.Development;
    }
}

builder.Configuration
    .AddJsonFile(Path.Combine("backend", "config", "appsettings.json"), optional: true, reloadOnChange: true)
    .AddJsonFile(Path.Combine("backend", "config", $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Configure the web host to use a custom URL or port if provided.
var configuredUrls = builder.Configuration["urls"]
    ?? builder.Configuration["ASPNETCORE_URLS"]
    ?? $"http://localhost:{builder.Configuration["AMS_PORT"] ?? "5002"}";
builder.WebHost.UseUrls(configuredUrls);

// Add services to the container.
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
        return;
    }

    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

ValidateProductionConfiguration(builder.Configuration, builder.Environment, databaseProvider, jwtOptions);

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiJwtPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

// External providers (configure client id/secret in appsettings)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.SaveTokens = true;
    });
}

var msClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
var msClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
if (!string.IsNullOrEmpty(msClientId) && !string.IsNullOrEmpty(msClientSecret))
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
    {
        options.ClientId = msClientId;
        options.ClientSecret = msClientSecret;
        options.SaveTokens = true;
    });
}

builder.Services.AddLocalization();
builder.Services.Configure<FileUploadValidationOptions>(builder.Configuration.GetSection("FileUpload"));
builder.Services.AddScoped<IAssistanceRequestFileService>(sp =>
{
    var uploadOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FileUploadValidationOptions>>().Value;
    return new AssistanceRequestFileService(uploadOptions);
});

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/frontend/pages/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/frontend/pages/Shared/{0}.cshtml");
        options.ViewLocationFormats.Add("/frontend/pages/{0}.cshtml");
    });
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AMS API",
        Version = "v1",
        Description = "Assistance Management System REST API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("ar-EG"),
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar-EG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AMS API v1");
    });
}

app.UseHttpsRedirection();

// Serve frontend static assets. When running from the built DLL the ContentRootPath
// may be the bin folder; try the project-root relative path as a fallback.
var staticAssetsPath = Path.Combine(builder.Environment.ContentRootPath, "frontend", "assets");
if (!Directory.Exists(staticAssetsPath))
{
    var alt = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "frontend", "assets"));
    if (Directory.Exists(alt))
    {
        staticAssetsPath = alt;
    }
}

if (Directory.Exists(staticAssetsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(staticAssetsPath),
        RequestPath = string.Empty
    });
}

var uploadsPath = Path.Combine(builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
}
if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}
app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();

static void ValidateProductionConfiguration(IConfiguration configuration, IHostEnvironment environment, string databaseProvider, JwtOptions jwtOptions)
{
    if (!environment.IsProduction())
    {
        return;
    }

    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) &&
        string.IsNullOrWhiteSpace(configuration.GetConnectionString("SqlServerConnection")))
    {
        throw new InvalidOperationException("Production requires ConnectionStrings:SqlServerConnection to be configured.");
    }

    if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) ||
        jwtOptions.SecretKey.Contains("CHANGE_THIS", StringComparison.OrdinalIgnoreCase) ||
        jwtOptions.SecretKey.Contains("PLEASE_CHANGE_THIS", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Production requires a real Jwt:SecretKey configured through user secrets or environment variables.");
    }

    var googleClientId = configuration["Authentication:Google:ClientId"];
    var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        throw new InvalidOperationException("Production requires Google OAuth credentials configured through user secrets or environment variables.");
    }

    var microsoftClientId = configuration["Authentication:Microsoft:ClientId"];
    var microsoftClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
    if (string.IsNullOrWhiteSpace(microsoftClientId) || string.IsNullOrWhiteSpace(microsoftClientSecret))
    {
        throw new InvalidOperationException("Production requires Microsoft OAuth credentials configured through user secrets or environment variables.");
    }
}

public partial class Program { }
