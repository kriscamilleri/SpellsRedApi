using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonFlatFileDataStore;
using SpellsRedApi.Models;
using SpellsRedApi.Models.Giddy;
namespace SpellsRedApi.Api
{
    public class RedSpellApi : IApi
    {
        public RedSpellApi(ApiProperties properties)
            : base(properties) { }

        IResult GetUser(int id)
        {
            RedSpell result = new RedSpell();

            //TODO: list all json files, without extension
            var repoList = new string[] { "PHB" };
            foreach(var repoFileName in repoList)
            {
                RedSpell[] redSpells = Array.Empty<RedSpell>();
                using (var store = GetDataStore(repoFileName, false))
                {
                    var spells = store.GetCollection<Spell>().AsQueryable();
                    redSpells = spells.Select((spell, i) => new RedSpell(spell, i)).ToArray();
                }
                using (var store = GetDataStore(repoFileName, true))
                {
                    store.GetCollection<RedSpell>().InsertMany(redSpells);
                }
            }

            return Results.Json(result, _jsonOptions);
        }

        //Import and generate Red Repository

        //Import Giddy Spell

        //Generate Red Repos for all giddy spells


        public override void SetRoutes()
        {
            _app.MapGet("/user/{id}", GetUser).RequireAuthorization();
        }

    }
}

