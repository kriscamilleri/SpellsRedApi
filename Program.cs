using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using SpellsRedApi.Api;
using SpellsRedApi.Routes;
using System.Security.Claims;
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
    // o.Authority = configuration["Jwt:Authority"];
    // o.Audience = configuration["Jwt:Audience"];

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

builder.Services.AddAuthentication(authOptions);//.AddJwtBearer(jwtOptions);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

var app = builder.Build();
app.UseCors(corsOptions);
app.UseHttpsRedirection();
app.UseAuthentication(); //uncomment me
app.UseAuthorization();  //uncomment me

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

var apiProps = new ApiProperties(app, jsonOptions, configuration);
new RedSpellApi(apiProps).SetRoutes();
new GiddySpellApi(apiProps).SetRoutes();
new LegacySpellApi(apiProps).SetRoutes();
new RepositoryApi(apiProps).SetRoutes();
new UserApi(apiProps).SetRoutes();

app.Run();



public class ClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;

        // flatten realm_access because Microsoft identity model doesn't support nested claims
        // by map it tow Microsoft identity model, because automatic JWT bearer token mapping already processed here
        if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "realm_access"))
        {
            var realmAccessClaim = claimsIdentity.FindFirst((claim) => claim.Type == "realm_access");
            var realmAccessAsDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(realmAccessClaim.Value);
            if (realmAccessAsDict["roles"] != null)
            {
                foreach (var role in realmAccessAsDict["roles"])
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        return Task.FromResult(principal);
    }
}