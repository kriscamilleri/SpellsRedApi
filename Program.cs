using JsonFlatFileDataStore;
using SpellsRedApi;
using SpellsRedApi.Models.Giddy;
using SpellsRedApi.Models.Legacy;
using SpellsRedApi.Models.Red;
using SpellsRedApi.Routes;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(builder => builder
 .AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader());

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

var routes = new Routes(app, jsonOptions, configuration["RepositoryPath"]);

app.Run();