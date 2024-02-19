using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
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

var assemblyConfigurationAttribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;
//DB
if (buildConfigurationName == "Release")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:dbProd"]));
    Debug.WriteLine("USING LIVE DB");
}
else if (buildConfigurationName == "Debug")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:dbDev"]));
    Debug.WriteLine("USING DEV DB");

}

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    await IdentityDataInitializer.SeedData(userManager,roleManager);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during seeding");
}

app.Run();
