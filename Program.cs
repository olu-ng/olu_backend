//using System.Text;
//using System.Reflection;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using OluBackendApp.Data;
//using OluBackendApp.Models;
//using OluBackendApp.Services;
//using System;

//var builder = WebApplication.CreateBuilder(args);

//// 1) Swagger services (XML comments + JWT support)
//// Force Kestrel to listen on all network interfaces, on port 5000
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(5000); // Use your desired port
//});
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(options =>
//{
//    // XML docs
//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    options.IncludeXmlComments(xmlPath);

//    // Swagger doc info
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Olu App API",
//        Version = "v1",
//        Description = "API documentation for Olu App"
//    });

//    // JWT Bearer setup for Swagger UI
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer {token}'",
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });
//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        [new OpenApiSecurityScheme
//        {
//            Reference = new OpenApiReference
//            {
//                Type = ReferenceType.SecurityScheme,
//                Id = "Bearer"
//            }
//        }
//        ] = new string[] { }
//    });
//});

//// 2) EF Core & Identity
////builder.Services.AddDbContext<ApplicationDbContext>(opts =>
////    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

////builder.Services.AddDbContext<ApplicationDbContext>(options =>
////    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
//{
//    // email & password policy...
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//// 3) JWT authentication
//var jwt = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwt["Key"]!);
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opts =>
//    {
//        opts.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(key),
//            ValidateIssuer = true,
//            ValidIssuer = jwt["Issuer"],
//            ValidateAudience = true,
//            ValidAudience = jwt["Audience"],
//            ValidateLifetime = true
//        };
//    });
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowLocalhost8081",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:8081")
//                  .AllowAnyHeader()
//                  .AllowAnyMethod();
//        });
//});

//// 4) Your application services & controllers
//builder.Services
//    .AddScoped<IEmailService, EmailService>()
//    .AddScoped<IOtpService, OtpService>()
//    .AddSingleton<ITokenService, TokenService>()
//    .AddScoped<IRefreshTokenService, RefreshTokenService>();
//builder.Services.AddControllers();

//var app = builder.Build();

//// 5) Swagger middleware (enable in all envs or wrap in if: app.Environment.IsDevelopment())
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Olu App API V1");
//    c.RoutePrefix = string.Empty;  // serve at root "/"
//});

//// 6) Kestrel listening and middleware
////app.Urls.Add("http://+:80");
//app.UseCors("AllowLocalhost8081");  // 
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//// 7) Role seeding
//using (var scope = app.Services.CreateScope())
//{
//    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    foreach (var role in new[] { "Admin", "Artisan", "OfficeOwner", "SuperAdmin" })
//        if (!await rm.RoleExistsAsync(role))
//            await rm.CreateAsync(new IdentityRole(role));
//}

//app.MapGet("/health", () => Results.Ok("Healthy"));
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.UseSwagger();
//app.UseSwaggerUI();
//app.Run();







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

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------
// 1) Kestrel port
// ------------------------------------------
builder.WebHost.ConfigureKestrel(opts =>
    opts.ListenAnyIP(5000)
);

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
builder.Services.AddSwaggerGen(opts =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    opts.IncludeXmlComments(xmlPath);

    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Olu App API",
        Version = "v1",
        Description = "API documentation for Olu App"
    });

    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
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
builder.Services.AddControllers()
       .AddNewtonsoftJson();

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


































//using System.Text;
//using System.Reflection;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.Extensions.FileProviders;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Microsoft.AspNetCore.Mvc;
//using OluBackendApp.Data;
//using OluBackendApp.Models;
//using OluBackendApp.Services;
//using Microsoft.Extensions.Options;
//using Microsoft.Extensions.DependencyInjection; // for AddHealthChecks()
//using Microsoft.Extensions.Diagnostics.HealthChecks; // for AddDbContextCheck<>

//var builder = WebApplication.CreateBuilder(args);

//// ---------------------------
//// 1) Database & Identity
//// ---------------------------
//builder.Services.AddDbContext<ApplicationDbContext>(opts =>
//    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
//{
//    opts.User.RequireUniqueEmail = true;

//    // Password policy
//    opts.Password.RequireDigit = true;
//    opts.Password.RequireLowercase = true;
//    opts.Password.RequireUppercase = true;
//    opts.Password.RequireNonAlphanumeric = true;
//    opts.Password.RequiredLength = 6;

//    // Lockout policy
//    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    opts.Lockout.MaxFailedAccessAttempts = 5;
//    opts.Lockout.AllowedForNewUsers = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//// ---------------------------
//// 2) JWT Authentication
//// ---------------------------
//var jwtSection = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(opts =>
//{
//    opts.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(key),
//        ValidateIssuer = true,
//        ValidIssuer = jwtSection["Issuer"],
//        ValidateAudience = true,
//        ValidAudience = jwtSection["Audience"],
//        ValidateLifetime = true
//    };
//});

//// ---------------------------
//// 3) CORS
//// ---------------------------
//builder.Services.AddCors(opts =>
//{
//    opts.AddPolicy("AllowLocalhost8081", policy =>
//        policy.WithOrigins("http://localhost:8081")
//              .AllowAnyHeader()
//              .AllowAnyMethod());
//});

//// ---------------------------
//// 4) Swagger + JWT in UI
//// ---------------------------
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(opts =>
//{
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
//                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//            },
//            Array.Empty<string>()
//        }
//    });
//});

//// ---------------------------
//// 5) DI: application services
//// ---------------------------
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<IOtpService, OtpService>();
//builder.Services.AddSingleton<ITokenService, TokenService>();
//builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

//// ---------------------------
//// 6) Controllers & JSON‑Patch
//// ---------------------------
//builder.Services.AddControllers()
//       .AddNewtonsoftJson();

//// ---------------------------
//// 7) Health checks
//// ---------------------------
//builder.Services.AddHealthChecks()
//       .AddDbContextCheck<ApplicationDbContext>("Database");

//// ---------------------------
//// 8) API versioning
//// ---------------------------
//builder.Services.AddApiVersioning(opts =>
//{
//    opts.AssumeDefaultVersionWhenUnspecified = true;
//    opts.DefaultApiVersion = new ApiVersion(1, 0);

//});

//// ---------------------------
//// 9) Form options (max 10 MB)
//// ---------------------------
//builder.Services.Configure<FormOptions>(opts =>
//{
//    opts.MultipartBodyLengthLimit = 10 * 1024 * 1024;
//});

//var app = builder.Build();

//// ---------------------------
//// Middleware pipeline
//// ---------------------------

//// Global error handler (non‑dev)
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/error");
//}
//app.UseHttpsRedirection();

//// Serve uploads securely
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "uploads")),
//    RequestPath = "/uploads",
//    ServeUnknownFileTypes = false
//});

//app.UseRouting();
//app.UseCors("AllowLocalhost8081");

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//app.MapHealthChecks("/healthz");

//// Problem details endpoint
//app.Map("/error", (HttpContext ctx) => Results.Problem("An unexpected error occurred."));

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Olu App API V1");
//        c.RoutePrefix = string.Empty;
//    });
//}

//// lightweight health check
//app.MapGet("/health", () => Results.Ok("Healthy"));

//// ---------------------------
//// Role seeding
//// ---------------------------
//using (var scope = app.Services.CreateScope())
//{
//    var roles = new[] { Roles.Admin, Roles.Artisan, Roles.OfficeOwner, Roles.SuperAdmin };
//    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    foreach (var role in roles)
//        if (!await rm.RoleExistsAsync(role))
//            await rm.CreateAsync(new IdentityRole(role));
//}

//app.Run();
