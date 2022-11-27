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

        private DataStore GetDataStore(string filename, bool isRedSpell = false)
        {
            if (isRedSpell)//TODO: replace me with specifier for folder rather than hardcoded only for redspell
            {
                return new DataStore($"Repositories/Red/{filename}.json"); //TODO: Clean filenames
            }
            return new DataStore($"Repositories/{filename}.json");
        }

        IResult GetSpells(string repository)
        {
            RedSpell[] redSpells = Array.Empty<RedSpell>();
            //TODO: list all json files, without extension
            var repoList = new string[] { "PHB" };
            foreach (var repoFileName in repoList)
            {
                using (var store = GetDataStore(repoFileName, true))
                {
                    redSpells = store.GetCollection<RedSpell>().AsQueryable().ToArray();
                }
            }

            return Results.Json(redSpells, _jsonOptions);
        }

        IResult ConvertFromGiddySpell(string repository)
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


        IResult GetRedSpells(string repository, HttpContext context)
        {
            string cleanRepository = Regex.Replace(repository, "[^A-Za-z0-9]", "");
            RedSpell[] results = Array.Empty<RedSpell>();
            using (var store = new DataStore($"Repositories/{cleanRepository}.json"))
            {
            }
            return Results.Json(results, _jsonOptions);
        }
        //Import and generate Red Repository

        //Import Giddy Spell

        //Generate Red Repos for all giddy spells


        public override void SetRoutes()
        {
            _app.MapGet("/redspell/giddyconvert/{repository}", ConvertFromGiddySpell);//.RequireAuthorization();
            _app.MapGet("/redspell/{repository}", GetSpells);
        }

    }
}

