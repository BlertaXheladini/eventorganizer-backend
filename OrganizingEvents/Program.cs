using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OrganizingEvents.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OrganizingEvents;
using Stripe;
using Microsoft.ML;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

MLContext context = new MLContext();
context.ComponentCatalog.RegisterAssembly(typeof(SeasonMappingFactory).Assembly);

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiting policy
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(), // Use the host IP Adress as the unique identifier
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(60) 
            }));

    // Add rate limiting headers
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers["Retry-After"] = "30";
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again in 30 seconds.", token);
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB ConnectionString
var mongoConnectionString = "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

// SQL ConnectionString
builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultDBConnection"));
    }
);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyWithAtLeast16Characters"))
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "AuthDemo",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Inicializoni elsin sekret t Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure CORS
app.UseCors(policy => policy.AllowAnyHeader()
                               .AllowAnyMethod()
                               .SetIsOriginAllowed(origin => true)
                               .AllowCredentials()
);

// Add security headers middleware
app.Use(async (context, next) =>
{
    // Add security headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self'; " +
        "connect-src 'self' https:; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'");

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
