
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Api;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
using SpellsRedApi.Models.Legacy;
using SpellsRedApi.Models.Red;
namespace SpellsRedApi.Routes
{
    public class RepositoryApi : IApi
    {
        public RepositoryApi(ApiProperties properties) : base(properties) { }

        private DataStore GetDataStore(string filename, bool isRedSpell = false)
        {
            var path = isRedSpell ? _paths.RedSpell : _paths.GiddySpell;
            return new DataStore($"{path}/{filename}.json");
        }

        public override void SetRoutes()
        {
            //Create Repositoryd
            _app.MapPut("/repository", CreateRepository);

            //List Repository information
            _app.MapGet("/repository/red", GetRedRepositoryList);
            _app.MapGet("/repository/giddy", GetGiddyRepositoryList);
            _app.MapGet("/repository/", GetRepositoryList);

            //Convert from Giddy to Red Repo
            _app.MapGet("/repository/giddytored/", GenerateAllRedFromGiddyRepositories);
            _app.MapGet("/repository/giddytored/{repository}", GenerateRedFromGiddyRepository);
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

            var uploadsPath = _paths.Repository;
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

        IResult GenerateRedFromGiddyRepository(string repository)
        {
            RedSpell[] redSpells = Array.Empty<RedSpell>();
            using (var store = GetDataStore(repository))
            {
                var spells = store.GetCollection<Spell>().AsQueryable();
                redSpells = spells.Select((spell, i) => new RedSpell(spell, i)).ToArray();
            }
            using (var store = GetDataStore(repository, true))
            {
                var success = store.InsertItem<RedSpell[]>("RedSpell", redSpells);
            }

            return Results.Json(redSpells.Count(), _jsonOptions);
        }

        IResult GenerateAllRedFromGiddyRepositories()
        {
            var uploadsPath = _paths.GiddySpell;
            var uploads = Directory.GetFiles(uploadsPath, "*.json").Select(c => Path.GetFileNameWithoutExtension(c)).ToArray();
            var completionList = new Dictionary<string, int>();
            foreach (var repository in uploads)
            {
                try
                {
                    RedSpell[] redSpells = Array.Empty<RedSpell>();
                    using (var store = GetDataStore(repository))
                    {
                        var spells = store.GetCollection<Spell>().AsQueryable();
                        redSpells = spells.Where(c => c.Name != null).Select((spell, i) => new RedSpell(spell, i)).ToArray();
                    }
                    using (var store = GetDataStore(repository, true))
                    {
                        var success = store.InsertItem<RedSpell[]>("RedSpell", redSpells);
                    }
                    completionList.Add(repository, redSpells.Count());
                }
                catch (Exception e)
                {
                    completionList.Add(repository, -1);

                }
            }

            return Results.Json(completionList, _jsonOptions);
        }
        IResult GetRepositoryList()
        {
            var uploadsPath = _paths.Repository;
            var uploads = Directory.GetFiles(uploadsPath, "*.json", SearchOption.AllDirectories);
            var results = uploads.Select(c => new Repository
            {
                Name = Path.GetFileNameWithoutExtension(c),
                Path = c,
                Source = "FileSystem"
            }).ToArray();
            return Results.Json(results, _jsonOptions);
        }

        IResult GetRedRepositoryList()
        {
            var uploadsPath = _paths.RedSpell;
            var uploads = Directory.GetFiles(uploadsPath, "*.json");
            var results = uploads.Select(c => new Repository
            {
                Name = Path.GetFileNameWithoutExtension(c),
                Path = c,
                Source = "FileSystem"
            }).ToArray();
            return Results.Json(results, _jsonOptions);
        }

        IResult GetGiddyRepositoryList()
        {
            var uploadsPath = _paths.GiddySpell;
            var uploads = Directory.GetFiles(uploadsPath, "*.json");
            var results = uploads.Select(c => new Repository
            {
                Name = Path.GetFileNameWithoutExtension(c),
                Path = c,
                Source = "FileSystem"
            }).ToArray();
            return Results.Json(results, _jsonOptions);
        }

    }
}