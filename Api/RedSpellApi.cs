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

        private DataStore GetDataStore(string repository, bool isRed)
        {
            return isRed
                ? new DataStore($"Repositories/Red/{repository}.json")
                : new DataStore($"Repositories/{repository}.json");
        }

        IResult GetSpell(int id, string repository)
        {
            RedSpell result = new RedSpell();
            using (var store = GetDataStore(repository, true))
            {
                result = store.GetCollection<RedSpell>().AsQueryable().First(c => c.Id == id);
            }
            return Results.Json(result, _jsonOptions);
        }

        IResult GetRedSpells(string repository, HttpContext context)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");
            RedSpell[] spells = Array.Empty<RedSpell>();
            using (var store = GetDataStore(repository, true))
            {
                 spells = store.GetCollection<RedSpell>().AsQueryable().ToArray();
            }
            return Results.Json(spells, _jsonOptions);
        }

        IResult ImportAllGiddySpells()
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
            _app.MapGet("/repository/{repository}/{id}", GetSpell).RequireAuthorization();
            _app.MapGet("/repository/ImportAll", ImportAllGiddySpells).RequireAuthorization();
        }

    }
}

