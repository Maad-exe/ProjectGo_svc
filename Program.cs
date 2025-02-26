using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Contracts;
using backend.Infrastructure.Repositories;
using backend.Infrastructure.Services.Contracts;
using backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using backend.Core.Settings;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var configuration = builder.Configuration;

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Settings from appsettings.json
var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings?.Secret ??
    throw new InvalidOperationException("JWT Secret not configured"));

builder.Services.AddSingleton(jwtSettings);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Add Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
         Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            // Add these crucial lines
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentPolicy", policy =>
        policy.RequireClaim("role", "Student"));
});

// Register Repositories and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add API Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

                                                                                                            app.UseHttpsRedirection();
                                                                                                            app.UseCors("AllowAngular");
                                                                                                            app.UseAuthentication();
                                                                                                            app.UseAuthorization();
                                                                                                            app.MapControllers();

                                                                                                            app.Run();
