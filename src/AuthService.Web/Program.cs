using System.Text;
using AuthService.Web.Core.Authorization;
using AuthService.Web.Core.Common;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Core.Options;
using AuthService.Web.Core.Services;
using AuthService.Web.Extensions;
using AuthService.Web.Infrastructure.Data;
using AuthService.Web.Middleware;
using Scalar.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new OptionalJsonConverterFactory());
});

builder.AddServiceDefaults();

// Key Vault is only wired in when deployed (AppHost only references "kv" in publish mode)
if (builder.Configuration.GetConnectionString("kv") is not null)
{
    builder.Configuration.AddAzureKeyVaultSecrets("kv");
}

// Local override: Web's own file/user-secrets config wins over the AppHost-injected
// local-container connection string, or over Key Vault when deployed.
var localConfig = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var localConnectionString = localConfig.GetConnectionString("AuthDB");
if (!string.IsNullOrEmpty(localConnectionString))
{
    builder.Configuration["ConnectionStrings:AuthDB"] = localConnectionString;
}

builder.AddNpgsqlDbContext<AppDbContext>("AuthDB");

builder.Services.AddProblemDetails(configure =>
{
    configure.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpoints();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IMessageService, MessageService>();

var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>()!;
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, SuperAdminAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstants.AdminOnly, policy =>
        policy.AddRequirements(new AdminRequirement()));

    options.AddPolicy(PolicyConstants.SuperAdminOnly, policy =>
        policy.AddRequirements(new SuperAdminRequirement()));
});

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("AuthService API")
        .WithPreferredScheme("Bearer")
        .WithHttpBearerAuthentication(bearer => bearer.Token = string.Empty);
});

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
