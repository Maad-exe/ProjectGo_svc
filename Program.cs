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
//using System.Security.Claims;
//using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
 .LogTo(Console.WriteLine, LogLevel.Debug));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
var jwtSecret = configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret key is not configured");
}
var key = Encoding.ASCII.GetBytes(jwtSecret);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        // Make sure these match the claims you create in GenerateJwtToken
        RoleClaimType = "role",
        NameClaimType = "sub"
    };
    // Critical: Match these to how you're creating your tokens
    // RoleClaimType = ClaimTypes.Role, // Match this to your token's claim type for roles
    //   NameClaimType = JwtRegisteredClaimNames.Sub,

    // Be lenient during troubleshooting
    // RequireSignedTokens = true,
    //   ValidateActor = false
    // };

    // Add debug handlers
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"Authentication failed: {context.Exception}");
            return Task.CompletedTask;
           

        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            Console.WriteLine($"Identity name: {context.Principal?.Identity?.Name}");
            Console.WriteLine("Claims in the token:");
            if (context.Principal?.Claims != null)
            {
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
                }
            }
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"Token received: {context.Token?.Substring(0, 20)}...");
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentPolicy", policy =>
        policy.RequireClaim("role", "Student"));
});


// Register Services and Repositories
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Add if needed
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

// These two must be in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();