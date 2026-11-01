using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PostsAPI.Middlewares;
using PostsAPI.Models;
using PostsAPI.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PostsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<RequestTimingOptions>(builder.Configuration.GetSection(RequestTimingOptions.RequestTiming));

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IPostService, PostService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters { 
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("Admin"));

builder.Services.AddOpenApi("internal");

builder.Services.AddHealthChecks();

// TESTING MY CUSTOM DI


//var container = new MiniContainer();
//container.Register<IEmailSender, FakeEmailSender>();
//container.ResolveSingleton<OrderService>().Place();
//container.ResolveSingleton<OrderService>().Place();
//container.ResolveSingleton<OrderService>().Place();
// END TEST


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.yml");
}
app.UseRequestTiming();

app.UseExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }