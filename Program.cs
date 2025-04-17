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
using backend.Hubs;
using backend.UnitOfWork;
using backend.UnitOfWork.Contract;


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
              .AllowCredentials().SetIsOriginAllowed(origin => true); // Be careful with this in production
    });
});

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
    hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(15);
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
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    };
    
    // Add debug handlers
    options.Events = new JwtBearerEvents
    {

        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/api/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

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
       
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{

    options.AddPolicy("StudentPolicy", policy =>
        policy.RequireRole("Student"));

    options.AddPolicy("TeacherPolicy", policy =>
        policy.RequireRole("Teacher"));

    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});


// Register all repositories 
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IPanelRepository, PanelRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IRubricRepository, RubricRepository>();

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IPanelService, PanelService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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
app.Use(async (context, next) =>
{
    var token = context.Request.Query["access_token"];
    if (!string.IsNullOrEmpty(token) && context.Request.Path.StartsWithSegments("/api/chatHub"))
    {
        context.Request.Headers.Add("Authorization", $"Bearer {token}");
    }

    await next();
});
app.MapHub<ChatHub>("/api/chatHub");
app.MapControllers();





app.Run();