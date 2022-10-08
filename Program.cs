using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Logging;
using SpellsRedApi.Api;
using SpellsRedApi.Routes;
using System.Text.Json;
using System.Text.Json.Serialization;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);

var authOptions = new Action<AuthenticationOptions>(c =>
{
    c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

var jwtOptions = new Action<JwtBearerOptions>(o =>
{
    o.Authority = configuration["Jwt:Authority"];
    o.Audience = configuration["Jwt:Audience"];
    
    if (builder.Environment.IsDevelopment())
    {
        o.RequireHttpsMetadata = false;
        o.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = c =>
            {
                return c.Response.WriteAsync(c.Exception.ToString());
            }
        };
    }
});

var corsOptions = new Action<CorsPolicyBuilder>(c =>
{
    c.AllowAnyMethod();
    c.AllowAnyHeader();
    c.AllowAnyOrigin();
});

var jsonOptions = new JsonSerializerOptions()
{
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

jsonOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services.AddAuthentication(authOptions).AddJwtBearer(jwtOptions);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors(corsOptions);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        o.RoutePrefix = string.Empty;
    });
    
    IdentityModelEventSource.ShowPII = true;
};

var apiProps = new ApiProperties(app, jsonOptions, configuration["RepositoryPath"]);
new Routes(apiProps).SetRoutes();
new UserApi(apiProps).SetRoutes();

app.Run();