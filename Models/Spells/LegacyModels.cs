﻿using SpellsRedApi.Models.Giddy;
using System.Globalization;

namespace SpellsRedApi.Models.Legacy
{
    public partial class LegacySpell
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string? Material { get; set; }
        public string Range { get; set; }
        public string Page { get; set; }
        public string Duration { get; set; }
        public string Casting { get; set; }
        public string Level { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
        public string Components { get; set; }
        public string? Higher { get; set; }
        public bool? Ritual { get; set; }
        public bool? Conc { get; set; }

        public LegacySpell() { }

        public LegacySpell(Spell spell, int id = 0)
        {
            TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;

            this.Id = id;
            setLevel(spell);
            this.Name = spell.Name;
            this.Material = nullIfEmpty(spell.Components.M?.String ?? "");
            this.Page = $"{spell.Source} {spell.Page}";
            this.Range =  textInfo.ToTitleCase($"{spell.Range?.Distance?.Amount ?? null} {spell.Range?.Distance?.Type ?? null} ({spell.Range?.Type ?? ""})".Trim());
            this.Desc = string.Join(" <br> ", spell.Entries.Select((c, i) => c.String));
            this.Casting = textInfo.ToTitleCase($"{spell.Time[0].Number} {spell.Time[0].Unit}");
            this.Higher = nullIfEmpty(string.Join(" <br> ", spell.EntriesHigherLevel?.Select(c => c.String.Replace("At Higher Levels: ", "")) ?? new List<string>()));
            setSchool(spell);
            this.Class = string.Join(", ", spell.Classes?.FromClassList?.Select(c => c.Name)
                ?? spell.Classes?.FromClassListVariant?.Select(c => c.Name)
                ?? spell.Classes?.FromSubclass?.Select(c => c.Class.Name)
                ?? new List<string>().Distinct());
            this.Ritual = spell.Meta?.Ritual;
            this.Conc = spell.Duration[0].Concentration;
            this.Duration = textInfo.ToTitleCase(spell.Duration[0]?.Duration != null ? $"{spell.Duration[0].Duration.Amount} {spell.Duration[0].Duration.Type}" : spell.Duration[0].Type);
            this.Components = string.Join(", ",
                new List<string>() {
                    spell.Components.V.HasValue ? "V" : string.Empty,
                    spell.Components.S.HasValue ? "S" : string.Empty,
                    spell.Components.M.HasValue ? "M" : string.Empty
                }
                .Where(c => !string.IsNullOrEmpty(c))
            );

        }
        
        private void setSchool(Spell spell)
        {
            var school = spell.School;
            switch (spell.School)
            {
                case "A":
                    school = "Abjuration";
                    break;
                case "B":
                    school = "Divination";
                    break;
                case "C":
                    school = "Enchantment";
                    break;
                case "D":
                    school = "Conjuration";
                    break;
                case "E":
                    school = "Abjuration";
                    break;
                case "F":
                    school = "Evocation";
                    break;
                case "G":
                    school = "Illusion";
                    break;
                case "H":
                    school = "Necromancy";
                    break;
                case "I":
                    school = "Transmutation";
                    break;
            }
            this.School = school;
        }

        private void setLevel(Spell spell) {
            var suffix = "";
            switch (spell.Level % 10)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
            this.Level = spell.Level == 0 ? "Cantrip" : $"{spell.Level}{suffix} Level";
        }

        private string? nullIfEmpty(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return value;
        }

        private bool isPropertyExist(dynamic settings, string name)
        {
            return ((Type)settings.GetType()).GetProperties().Where(p => p.Name.Equals(name)).Any();
        }

    }
}
