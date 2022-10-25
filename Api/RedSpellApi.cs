using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
using SpellsRedApi.Models.Red;

namespace SpellsRedApi.Api
{
    public class RedSpellApi : IApi
    {
        public RedSpellApi(ApiProperties properties)
            : base(properties) { }


        IResult GetLegacySpellAsRedSpell(string repository, HttpContext context)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");
            RedSpell[] results = Array.Empty<RedSpell>();
            using (var store = new DataStore($"Repositories/{cleanRepository}.json"))
            {
                var spells = store.GetCollection<Spell>().AsQueryable();
                results = spells.Select((spell, i) => new RedSpell(spell, i)).ToArray();
            }
            return Results.Json(results, _jsonOptions);
        }

        public override void SetRoutes()
        {
            _app.MapGet("/redspell/{repository}", GetLegacySpellAsRedSpell).RequireAuthorization();
        }

    }
}
