using JsonFlatFileDataStore;
using SpellsRedApi;
using System.Text.Json;
using System.Text.RegularExpressions;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var jsonOptions = new JsonSerializerOptions()
{
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};

app.MapPut("/repository", async (string name, string source, HttpRequest req) =>
{
    if (!req.HasFormContentType)
    {
        return Results.BadRequest();
    }

    var form = await req.ReadFormAsync();
    var file = form.Files["file"];

    if (file is null)
    {
        return Results.BadRequest();
    }

    var uploadsPath = configuration["RepositoryPath"];
    string cleanName = Regex.Replace(name, "[^A-Za-z0-9]", "");
    var uploads = Path.Combine(uploadsPath, $"{cleanName}.json");
    await using var fileStream = File.OpenWrite(uploads);
    await using var uploadStream = file.OpenReadStream();
    await uploadStream.CopyToAsync(fileStream);

    using (var store = new DataStore($"repositories.json"))
    {
        var collection = store.GetCollection<Repository>();

        var results = collection.InsertOne(
            new Repository
            {
                Name = name,
                Path = uploads,
                Source = source
            });
    }

    return Results.NoContent();
})
.WithName("CreateRepository");

app.MapGet("/repository",  () =>
{
    Repository[] results = Array.Empty<Repository>();
    using (var store = new DataStore($"repositories.json"))
    {
        results = store.GetCollection<Repository>().AsQueryable().ToArray();
    }
    return Results.Json(results, jsonOptions);
})
.WithName("GetRepositories");

app.MapGet("/repository/{repository}", (string repository) =>
{
    Repository[] results = Array.Empty<Repository>();
    using (var store = new DataStore($"repositories.json"))
    {
        results = store.GetCollection<Repository>().AsQueryable().Where(c=> c.Name == repository).ToArray();
    }
    return Results.Json(results, jsonOptions);
})
.WithName("GetRepository");

app.MapGet("/spell/{repository}", (string repository) =>
{
    string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

    Spell[] results = Array.Empty<Spell>();
    using (var store = new DataStore($"Repositories\\{cleanRepository}.json"))
    {
        results = store.GetCollection<Spell>().AsQueryable().ToArray();
    }
    return Results.Json(results, jsonOptions);
})
.WithName("GetSpells");


app.MapGet("/legacyspell/{repository}", (string repository) =>
{
    string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

    LegacySpell[] results = Array.Empty<LegacySpell>();
    using (var store = new DataStore($"Repositories\\{cleanRepository}.json"))
    {
        var spells = store.GetCollection<Spell>().AsQueryable();
        results = spells.Select(spell => new LegacySpell(spell)).ToArray();
    }
    return Results.Json(results, jsonOptions);
})
.WithName("GetLegacySpells");


app.MapGet("/spell/{repository}/{spell}", (string repository, string spell) =>
{
    string cleanSpell = Regex.Replace(spell, "[^A-Za-z0-9]", "");
    string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

    Spell? results = new Spell();
    var uploadsPath = configuration["RepositoryPath"];

    using (var store = new DataStore($"Repositories\\{cleanRepository}.json"))
    {
        results = store.GetCollection<Spell>().AsQueryable().FirstOrDefault(c => c.Name == spell);
    }
    return Results.Json(results, jsonOptions);
})
.WithName("GetSpell");


app.Run();