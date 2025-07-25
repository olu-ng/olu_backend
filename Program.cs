using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OluBackendApp.Data;
using OluBackendApp.Hubs;           // ← add this
using OluBackendApp.Models;
using OluBackendApp.Services;
using OluBackendApp.DTOs;
using Newtonsoft.Json.Converters;


var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------
// 1) Kestrel port
// ------------------------------------------
//builder.WebHost.ConfigureKestrel(opts =>
//    opts.ListenAnyIP(5000)
//);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // Only HTTP
});
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5000); // HTTP
//    options.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.UseHttps(); // ✅ Enables HTTPS for Swagger UI
//    });
//});

// ------------------------------------------
// 2) EF Core & Identity
// ------------------------------------------
// builder.Services.AddDbContext<ApplicationDbContext>(opts =>
//     opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
// );


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequiredLength = 6;

    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ------------------------------------------
// 3) JWT Authentication
// ------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true
        };
    });

// ------------------------------------------
// 4) CORS
// ------------------------------------------
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowLocalhost8081", policy =>
        policy.WithOrigins("http://localhost:8081")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ------------------------------------------
// 5) SignalR
// ------------------------------------------
builder.Services.AddSignalR();

// ------------------------------------------
// 6) Swagger + JWT in UI
// ------------------------------------------
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(opts =>
//{
//    // ✅ Prevent schema ID conflicts by using full type names
//    opts.CustomSchemaIds(type => type.FullName);

//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    opts.IncludeXmlComments(xmlPath);

//    opts.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Olu App API",
//        Version = "v1",
//        Description = "API documentation for Olu App"
//    });

//    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer {token}'",
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});

builder.Services.AddSwaggerGen(opts =>
{
    // Prevent schema ID conflicts by using full type names
    opts.CustomSchemaIds(type => type.FullName);

    // XML documentation file path (auto from project name)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    opts.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Swagger UI info
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Olu App API",
        Version = "v1",
        Description = "API documentation for Olu App",
        Contact = new OpenApiContact
        {
            Name = "Alpha Global Dev Team",
            Email = "support@alpha-global.org"
        }
    });

    // JWT Bearer token authentication
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Require token in all endpoints
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>() // ← No scopes required
        }
    });
});


// ------------------------------------------
// 7) App services
// ------------------------------------------
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// ------------------------------------------
// 8) MVC + NewtonsoftJson
// ------------------------------------------
//builder.Services.AddControllers()
//       .AddNewtonsoftJson();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
        options.SerializerSettings.Converters.Add(new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-dd"
        });
    });

//builder.AddControllers()
//    .AddNewtonsoftJson(options =>
//    {
//        options.SerializerSettings.Converters.Add(new StringEnumConverter());
//    });

// ------------------------------------------
// 9) Health checks
// ------------------------------------------
builder.Services.AddHealthChecks()
       .AddDbContextCheck<ApplicationDbContext>("Database");

// ------------------------------------------
// 10) API versioning
// ------------------------------------------
builder.Services.AddApiVersioning(opts =>
{
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.DefaultApiVersion = new ApiVersion(1, 0);
});

// ------------------------------------------
// 11) Form options (uploads up to 10 MB)
// ------------------------------------------
builder.Services.Configure<FormOptions>(opts =>
{
    opts.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// —— auto-create & migrate the Onu database ——
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ------------------------------------------
// 12) Global error handler & HTTPS
// ------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
app.UseHttpsRedirection();

// ------------------------------------------
// 13) Static files (uploads folder)
// ------------------------------------------
var webRoot = app.Environment.WebRootPath
           ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
webRoot = Path.GetFullPath(webRoot);

var uploadsDir = Path.Combine(webRoot, "uploads");
Directory.CreateDirectory(uploadsDir);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDir),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = false
});

// ------------------------------------------
// 14) Routing, CORS, Auth
// ------------------------------------------
app.UseRouting();
app.UseCors("AllowLocalhost8081");
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------
// 15) Swagger UI
// ------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Olu App API V1");
    c.RoutePrefix = string.Empty;
});



// ------------------------------------------
// 16) Map controllers & hubs
// ------------------------------------------
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapGet("/health", () => Results.Ok("Healthy"));
app.Map("/error", (HttpContext _) => Results.Problem("An unexpected error occurred."));
app.MapHub<ChatHub>("/hubs/chat");  // ← chat hub endpoint

// ------------------------------------------
// 17) Seed roles
// ------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var rm = scope.ServiceProvider
                  .GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] {
        Roles.Admin,
        Roles.Artisan,
        Roles.OfficeOwner,
        Roles.SuperAdmin
    })
    {
        if (!await rm.RoleExistsAsync(role))
            await rm.CreateAsync(new IdentityRole(role));
    }
}

app.Run();

