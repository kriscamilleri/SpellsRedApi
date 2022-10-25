
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
        public RepositoryApi(ApiProperties properties) : base(properties){}

        public override void SetRoutes()
        {
            _app.MapPut("/repository", CreateRepository);

            _app.MapGet("/repository", GetRepositories);

            _app.MapGet("/repository/{repository}", GetRepository);

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

            var uploadsPath = _repoPath;
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

        IResult GetRepositories()
        {
            Repository[] results = Array.Empty<Repository>();
            using (var store = new DataStore($"repositories.json"))
            {
                results = store.GetCollection<Repository>().AsQueryable().ToArray();
            }
            return Results.Json(results, _jsonOptions);
        }

        IResult GetRepository(string repository)
        {
            Repository[] results = Array.Empty<Repository>();
            using (var store = new DataStore($"repositories.json"))
            {
                results = store.GetCollection<Repository>().AsQueryable().Where(c => c.Name == repository).ToArray();
            }
            return Results.Json(results, _jsonOptions);
        }


    }
}