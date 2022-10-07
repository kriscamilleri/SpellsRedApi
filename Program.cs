using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using SpellsRedApi.Api;
using SpellsRedApi.Routes;
using System.Text.Json;
using System.Text.Json.Serialization;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           }).AddJwtBearer(o =>
           {
               if (builder.Environment.IsDevelopment())
               {
                   o.RequireHttpsMetadata = false;
               }
               o.Authority = configuration["Jwt:Authority"];
               o.Audience = configuration["Jwt:Audience"];
               o.Events = new JwtBearerEvents()
               {
                   //    OnTokenValidated = c =>
                   //    {
                   //        c.Success();
                   //        return c.Response.CompleteAsync();
                   //    },
                   OnAuthenticationFailed = c =>
                   {
                       c.NoResult();

                       c.Response.StatusCode = 500;
                       c.Response.ContentType = "text/plain";
                       if (builder.Environment.IsDevelopment())
                       {
                           return c.Response.WriteAsync(c.Exception.ToString());
                       }
                       return c.Response.WriteAsync("An error occured processing your authentication.");
                   }
               };
           });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();
app.UseCors(c=> c.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
IdentityModelEventSource.ShowPII = true;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
};

var jsonOptions = new JsonSerializerOptions()
{
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

jsonOptions.Converters.Add(new JsonStringEnumConverter());

new Routes(app, jsonOptions, configuration["RepositoryPath"]).SetRoutes();
new UserApi(app, jsonOptions, configuration["RepositoryPath"]).SetRoutes();

app.Run();