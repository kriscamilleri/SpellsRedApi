using SpellsRedApi.Models.Giddy;
using System.Globalization;
using static SpellsRedApi.Models.Red.Description;

namespace SpellsRedApi.Models.Red
{
    public class Description
    {
        public List<string>? Paragraph { get; set; }
        public DescriptionTable? Table { get; set; }
        public DescriptionList? List { get; set; }

        public class DescriptionList
        {
            public List<string>? Headers { get; set; }
            public List<string> Rows { get; set; }
            public DescriptionList(List<string> rows)
            {
                Rows = rows;
            }
        }

        public class DescriptionTable
        {
            List<string> Headers { get; set; }

            List<List<string>> Rows { get; set; }

            public DescriptionTable(List<string> headers, List<List<string>> rows)
            {
                Headers = headers;
                Rows = rows;
            }
        }

    }

    public class Range
    {
        public string? Type { get; set; }
        public RangeDistance? Distance { get; set; }

        public class RangeDistance
        {
            public string? Unit { get; set; }
            public int? Amount { get; set; }
        }

        public override string ToString()
        {
            return $"{this.Distance?.Amount ?? null} {this.Distance?.Unit ?? null} ({this.Type ?? ""})";
        }

    }

    public class Casting
    {
        public string Unit { get; set; }
        public int? Amount { get; set; }
        public string? Condition { get; set; }

        public Casting(string unit)
        {
            Unit = unit;
        }

        public override string ToString()
        {
            return $"{this.Unit} {this.Amount?.ToString() ?? string.Empty} {this.Condition?.ToString() ?? string.Empty}";
        }
    }

    public class Components
    {
        public string? Material { get; set; }
        public bool? IsVerbal { get; set; }
        public bool? IsSomatic { get; set; }
        public bool? IsMaterial { get; set; }
    }

    public class Duration
    {
        public string Type { get; set; }
        public int? Amount { get; set; }
        public List<string>? Ends { get; set; }
        public Duration(string type)
        {
            Type = type;
        }
        public override string ToString()
        {
            if (Ends == null)
            {
                return this.Amount != null ? $"{this.Amount} {this.Type}" : this.Type;
            }
            return this.Amount.HasValue ? $"{this.Amount} {this.Type}" : this.Type;
        }
    }

    public partial class RedSpell
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Page { get; set; }
        public int Level { get; set; }
        public string School { get; set; }
        public List<string> Class { get; set; }
        public bool? IsRitual { get; set; }
        public bool? IsConcentration { get; set; }
        public Range Range { get; set; }
        public Duration Duration { get; set; }
        public Casting Casting { get; set; }
        public Components Components { get; set; }
        public Description Description { get; set; }
        public Description? Higher { get; set; }

        public RedSpell() { }

        public RedSpell(Spell spell, int id = 0)
        {
            TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;

            this.Id = id;
            this.Name = spell.Name;
            this.Page = (int)spell.Page;
            this.IsRitual = spell.Meta?.Ritual;
            this.IsConcentration = spell.Duration[0].Concentration;
            setClass(spell);
            setSchool(spell);
            setLevel(spell);
            setComponents(spell);
            setRange(spell);
            setCasting(spell);
            setDuration(spell);
            setDescription(spell);
            setHigher(spell);
        }

        private void setHigher(Spell spell)
        {

            if (spell.EntriesHigherLevel != null && spell.EntriesHigherLevel.Any())
            {
                this.Higher = new Description()
                {
                    Paragraph = spell.EntriesHigherLevel.Select(c => c.String).ToList()
                };
            }
            
        }

        private void setDescription(Spell spell)
        {
            List<string> listRows = spell.Entries
                .Where(c => c.PurpleEntry != null && c.PurpleEntry.ListItems != null && c.PurpleEntry.ListItems.Count() > 0)
                .SelectMany(c => c.PurpleEntry.ListItems)
                .ToList();

            List<string>? listHeaders = 
                spell.Entries.Any(c => c.PurpleEntry != null && c.PurpleEntry.ListHeaders != null && c.PurpleEntry.ListHeaders.Count() > 0) 
                ? spell.Entries
                    .Where(c => c.PurpleEntry != null && c.PurpleEntry.ListHeaders != null && c.PurpleEntry.ListHeaders.Count() > 0)
                    .SelectMany(c => c.PurpleEntry.ListHeaders)
                    .ToList() 
                : null;

            DescriptionList? list = null;
            if (listRows.Count > 0)
            {
                 list = new DescriptionList(listRows);
                list.Headers = listHeaders;
            }

            var tableRows = spell.Entries
               .Where(c => c.PurpleEntry != null && c.PurpleEntry.TableRows != null && c.PurpleEntry.TableRows.Count() > 0)
               .SelectMany(c => c.PurpleEntry.TableRows)
               .ToList();

            List<string> tableHeaders =
              spell.Entries
                    .Where(c => c.PurpleEntry != null && c.PurpleEntry.TableHeaders != null && c.PurpleEntry.TableHeaders.Count() > 0)
                    .SelectMany(c => c.PurpleEntry.TableHeaders)
                    .ToList();

            DescriptionTable? table = null;
            if (tableRows.Count > 0)
            {
                table = new DescriptionTable(tableHeaders, tableRows);
            }

            var paragraph = spell.Entries?.Select(c => c.String).ToList() ?? new List<string>();

            this.Description = new Description()
            {
                List = list,
                Paragraph = paragraph,
                Table = table
            };
        }

        private void setDuration(Spell spell)
        {
            this.Duration = new Duration(spell.Duration[0]?.Duration?.Type ?? spell.Duration[0]?.Type ?? "");
            this.Duration.Amount = (int?)spell.Duration[0]?.Duration?.Amount;
            this.Duration.Ends = spell.Duration[0].Ends;
        }

        private void setCasting(Spell spell)
        {
            this.Casting = new Casting(spell.Time[0].Unit ?? "");
            this.Casting.Amount = (int?)spell.Time[0].Number;
            this.Casting.Condition = spell.Time[0].Condition;
        }

        private void setClass(Spell spell)
        {
            this.Class = spell.Classes?.FromClassList?.Select(c => c.Name).ToList()
                          ?? spell.Classes?.FromClassListVariant?.Select(c => c.Name).ToList()
                          ?? spell.Classes?.FromSubclass?.Select(c => c.Class.Name).ToList()
                          ?? new List<string>();
        }

        private void setRange(Spell spell)
        {
            this.Range = new Range();
            this.Range.Distance = new Range.RangeDistance();
            this.Range.Type = spell.Range.Type;
            this.Range.Distance.Amount = (int?)spell.Range?.Distance?.Amount ?? null;
            this.Range.Distance.Unit = spell.Range?.Distance?.Type;
        }


        private void setComponents(Spell spell)
        {
            this.Components = new Components();
            this.Components.Material = nullIfEmpty(spell.Components.M?.String ?? "");
            this.Components.IsMaterial = spell.Components.M.HasValue;
            this.Components.IsVerbal = spell.Components.V.HasValue;
            this.Components.IsSomatic = spell.Components.S.HasValue;
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


        private void setLevel(Spell spell)
        {
            this.Level = (int)spell.Level;
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
