namespace SpellsRedApi.Models.Legacy
{
    public partial class LegacySpell
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Material { get; set; }
        public string Range { get; set; }
        public string Page { get; set; }
        public string Duration { get; set; }
        public string Casting { get; set; }
        public string Level { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
        public string Components { get; set; }
        public string Higher { get; set; }
        public bool? Ritual { get; set; }
        public bool? Conc { get; set; }

        public LegacySpell(Spell spell, int id = 0)
        {
            this.Id = id;
            this.Name = spell.Name;
            this.Level = spell.Level == 0 ? "Cantrip" : spell.Level.ToString();
            this.Material = spell.Components.M?.String ?? "";
            this.Page = spell.Page.ToString();
            this.Range = spell.Range?.ToString() ?? "";
            this.Desc = string.Join(" <br> ", spell.Entries.Select(c => c.String));
            this.Casting = $"{spell.Time[0].Number} {spell.Time[0].Unit}";
            this.Higher = spell.EntriesHigherLevel != null && spell.EntriesHigherLevel.Any()
                ? string.Join(" <br> ", spell.EntriesHigherLevel.Select(c => c.Entries.Select(d => d)))
                : string.Empty;
            this.School = spell.School;
            this.Class = string.Join(", ", spell.Classes?.FromClassList?.Select(c => c.Name)
                ?? spell.Classes?.FromClassListVariant?.Select(c => c.Name)
                ?? spell.Classes?.FromSubclass?.Select(c => c.Class.Name)
                ?? new List<string>().Distinct());
            this.Ritual = spell.Meta?.Ritual;
            this.Conc = spell.Duration[0].Concentration;
            this.Components = string.Join(",",
                new List<string>() {
                    spell.Components.V.HasValue ? "V" : string.Empty,
                    spell.Components.S.HasValue ? "S" : string.Empty,
                    spell.Components.M.HasValue ? "M" : string.Empty
                }
                .Where(c => !string.IsNullOrEmpty(c))
            );

        }

        private bool IsPropertyExist(dynamic settings, string name)
        {
            return ((Type)settings.GetType()).GetProperties().Where(p => p.Name.Equals(name)).Any();
        }

    }
}
