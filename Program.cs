//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using OluBackendApp.Data;
//using OluBackendApp.Models;
//using OluBackendApp.Services;
//using Microsoft.OpenApi.Models;
////using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using System.Reflection;
////using OluBackendApp.Service;
//using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSwaggerGen(options =>
//{
//    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
//    options.IncludeXmlComments(xmlPath);
//});

//// 1) EF Core & Identity
//builder.Services.AddDbContext<ApplicationDbContext>(opts =>
//    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
//{
//    opts.User.RequireUniqueEmail = true;
//    // Password policy:
//    opts.Password.RequireDigit = true;
//    opts.Password.RequireLowercase = true;
//    opts.Password.RequireUppercase = true;
//    opts.Password.RequireNonAlphanumeric = true;
//    opts.Password.RequiredLength = 8;
//    // Lockout policy:
//    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    opts.Lockout.MaxFailedAccessAttempts = 5;
//    opts.Lockout.AllowedForNewUsers = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();


//// 2) JWT
//var jwt = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwt["Key"]!);
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
//        ValidIssuer = jwt["Issuer"],
//        ValidateAudience = true,
//        ValidAudience = jwt["Audience"],
//        ValidateLifetime = true
//    };
//});


//// 4) Swagger/OpenAPI
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Olu App API",
//        Version = "v1",
//        Description = "API documentation for Olu App",
//    });

//    // JWT Bearer Authentication setup for Swagger
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' followed by your JWT token.",
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
//            new string[] {}
//        }
//    });
//});



//// 3) App services
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<IOtpService, OtpService>();
//builder.Services.AddSingleton<ITokenService, TokenService>();
//builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
//builder.Services.AddControllers();

//var app = builder.Build();

//// Swagger UI setup
//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
//    builder.Services.AddEndpointsApiExplorer();
//    builder.Services.AddSwaggerGen();
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Olu App API V1");
//        options.RoutePrefix = string.Empty; // Serves Swagger at app root: http://localhost:5000/
//    });
//}



////–– Seed roles
//using (var scope = app.Services.CreateScope())
//{
//    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    string[] roles = { "Admin", "Artisan", "OfficeOwner", "SuperAdmin" };

//    foreach (var r in roles)
//    {
//        if (!await roleMgr.RoleExistsAsync(r))
//            await roleMgr.CreateAsync(new IdentityRole(r));
//    }
//}

//app.MapGet("/health", () => "Healthy");

//app.Urls.Add("http://+:80");  // <-- 👈 Make sure Kestrel listens on port 80
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();












using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OluBackendApp.Data;
using OluBackendApp.Models;
using OluBackendApp.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) Swagger services (XML comments + JWT support)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // XML docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Swagger doc info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Olu App API",
        Version = "v1",
        Description = "API documentation for Olu App"
    });

    // JWT Bearer setup for Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }
        ] = new string[] { }
    });
});

// 2) EF Core & Identity
//builder.Services.AddDbContext<ApplicationDbContext>(opts =>
//    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    // email & password policy...
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3) JWT authentication
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true
        };
    });

// 4) Your application services & controllers
builder.Services
    .AddScoped<IEmailService, EmailService>()
    .AddScoped<IOtpService, OtpService>()
    .AddSingleton<ITokenService, TokenService>()
    .AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddControllers();

var app = builder.Build();

// 5) Swagger middleware (enable in all envs or wrap in if: app.Environment.IsDevelopment())
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Olu App API V1");
    c.RoutePrefix = string.Empty;  // serve at root "/"
});

// 6) Kestrel listening and middleware
app.Urls.Add("http://+:80");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 7) Role seeding
using (var scope = app.Services.CreateScope())
{
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Artisan", "OfficeOwner", "SuperAdmin" })
        if (!await rm.RoleExistsAsync(role))
            await rm.CreateAsync(new IdentityRole(role));
}

app.MapGet("/health", () => Results.Ok("Healthy"));
app.Run();
