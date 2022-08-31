
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Models.Giddy;
using SpellsRedApi.Models.Legacy;
using SpellsRedApi.Models.Red;
namespace SpellsRedApi.Routes
{
    public class Routes
    {
        private WebApplication app { get; set; }

        private JsonSerializerOptions jsonOptions;
        private string repoPath;

        public Routes(WebApplication app, JsonSerializerOptions jsonOptions, string repoPath)
        {
            this.app = app;
            this.jsonOptions = jsonOptions;
            this.repoPath = repoPath;
            Map();
        }

        async Task<IResult> CreateRepository(string name, string source, HttpRequest req)
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

            var uploadsPath = repoPath;
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
        }
        async Task<IResult> GetRepositories()
        {
            Repository[] results = Array.Empty<Repository>();
            using (var store = new DataStore($"repositories.json"))
            {
                results = store.GetCollection<Repository>().AsQueryable().ToArray();
            }
            return Results.Json(results, jsonOptions);
        }
        async Task<IResult> GetRepository(string repository)
        {
            Repository[] results = Array.Empty<Repository>();
            using (var store = new DataStore($"repositories.json"))
            {
                results = store.GetCollection<Repository>().AsQueryable().Where(c => c.Name == repository).ToArray();
            }
            return Results.Json(results, jsonOptions);
        }
        async Task<IResult> GetSpells(string repository)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            Spell[] results = Array.Empty<Spell>();
            using (var store = new DataStore($"Repositories//{cleanRepository}.json"))
            {
                results = store.GetCollection<Spell>().AsQueryable().ToArray();
            }
            return Results.Json(results, jsonOptions);
        }
        async Task<IResult> GetLegacySpell(string repository)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            LegacySpell[] results = Array.Empty<LegacySpell>();
            using (var store = new DataStore($"Repositories/{cleanRepository}.json"))
            {
                var spells = store.GetCollection<Spell>().AsQueryable();
                results = spells.Select((spell, i) => new LegacySpell(spell, i)).ToArray();
            }
            return Results.Json(results, jsonOptions);
        }
        async Task<IResult> GetRedSpells(string repository)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            RedSpell[] results = Array.Empty<RedSpell>();
            using (var store = new DataStore($"Repositories/{cleanRepository}.json"))
            {
                var spells = store.GetCollection<Spell>().AsQueryable();
                results = spells.Select((spell, i) => new RedSpell(spell, i)).ToArray();
            }
            return Results.Json(results, jsonOptions);
        }
        async Task<IResult> GetSpell(string repository, string spell)
        {
            string cleanSpell = Regex.Replace(spell, "[^A-Za-z0-9]", "");
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            Spell? results = new Spell();
            var uploadsPath = repoPath;

            using (var store = new DataStore($"Repositories\\{cleanRepository}.json"))
            {
                results = store.GetCollection<Spell>().AsQueryable().FirstOrDefault(c => c.Name == spell);
            }
            return Results.Json(results, jsonOptions);
        }
        public void Map()
        {

            app.MapPut("/repository", CreateRepository);

            app.MapGet("/repository", () => GetRepositories);

            app.MapGet("/repository/{repository}", GetRepository);

            app.MapGet("/spell/{repository}", GetSpells);

            app.MapGet("/legacyspell/{repository}", GetLegacySpell);

            app.MapGet("/redspell/{repository}", GetRedSpells);

            app.MapGet("/spell/{repository}/{spell}", GetSpell);

        }

    }
}