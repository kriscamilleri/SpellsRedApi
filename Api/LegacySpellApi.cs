using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
using SpellsRedApi.Models.Legacy;
using SpellsRedApi.Models.Red;

namespace SpellsRedApi.Api
{
    public class LegacySpellApi : IApi
    {
        public LegacySpellApi(ApiProperties properties)
            : base(properties) { }


        IResult GetLegacySpell(string repository, HttpContext context)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");

            LegacySpell[] results = Array.Empty<LegacySpell>();
            using (var store = new DataStore($"Repositories/{cleanRepository}.json"))
            {
                var spells = store.GetCollection<Spell>().AsQueryable();
                results = spells.Select((spell, i) => new LegacySpell(spell, i)).ToArray();
            }
            return Results.Json(results, _jsonOptions);
        }

        public override void SetRoutes()
        {
            _app.MapGet("/legacyspell/{repository}", GetLegacySpell);
        }

    }
}

