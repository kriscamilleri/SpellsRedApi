using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Models.Giddy;

namespace SpellsRedApi.Api
{
    public class GiddySpellApi : IApi
    {
        public GiddySpellApi(ApiProperties properties) : base(properties) { }


        IResult GetSpells(string repository)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            Spell[] results = Array.Empty<Spell>();
            using (var store = new DataStore($"Repositories//{cleanRepository}.json"))
            {
                results = store.GetCollection<Spell>().AsQueryable().ToArray();
            }
            return Results.Json(results, _jsonOptions);
        }

        IResult GetSpell(string repository, string spell)
        {
            string cleanSpell = Regex.Replace(spell, "[^A-Za-z0-9]", "");
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            Spell? results = new Spell();

            using (var store = new DataStore($"Repositories\\{cleanRepository}.json"))
            {
                results = store.GetCollection<Spell>().AsQueryable().FirstOrDefault(c => c.Name == spell);
            }
            return Results.Json(results, _jsonOptions);
        }

        public override void SetRoutes()
        {
            _app.MapGet("/spell/{repository}", GetSpells);

            _app.MapGet("/spell/{repository}/{spell}", GetSpell);
        }

    }
}

