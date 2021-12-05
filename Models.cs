using Newtonsoft.Json.Linq;
using System.Dynamic;
using JsonFlatten;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpellsRedApi
{

    public partial class Repository
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Source { get; set; }

    }

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

        public LegacySpell(Spell spell)
        {
            this.Id = 0;
            this.Name = spell.Name;
            this.Level = spell.Level == 0 ? "Cantrip" : spell.Level.ToString();
            this.Material = spell.Components.M?.String ?? "";
            this.Page = spell.Page.ToString();
            this.Range = spell.Range?.ToString() ?? "";
            this.Desc = string.Join(" <br> ", spell.Entries.Select(c=>c.String));// parseDescription(spell);
            this.Casting = $"{spell.Time[0].Number} {spell.Time[0].Unit}";
            this.Higher = spell.EntriesHigherLevel != null && spell.EntriesHigherLevel.Any()
                ? string.Join(" <br> ", spell.EntriesHigherLevel.Select(c => c.Entries.Select(d => d)))
                : string.Empty;
            this.School = spell.School;
            this.Class = string.Join(", ", spell.Classes?.FromClassList?.Select(c => c.Name) ?? spell.Classes?.FromClassListVariant?.Select(c => c.Name) ?? spell.Classes?.FromSubclass?.Select(c => c.Class.Name) ?? new List<string>()) ;
            this.Ritual = spell.Meta?.Ritual;
            this.Ritual = spell.Duration[0].Concentration;
            this.Components = string.Join(",", new List<string>() {
                spell.Components.V.HasValue ? "V" : string.Empty,
                spell.Components.S.HasValue ? "S" : string.Empty,
                spell.Components.M.HasValue ? "M" : string.Empty
            }
            .Where(c => !string.IsNullOrEmpty(c)
            ));

        }

        private bool IsPropertyExist(dynamic settings, string name)
        {
            return ((Type)settings.GetType()).GetProperties().Where(p => p.Name.Equals(name)).Any();
        }


    }
    public partial class Welcome
    {
        public List<Spell> Spell { get; set; }
    }

    public partial class SpellEntryUIList
    {
     
        [Required]
        public string Type { get; set; }
        public string Style { get; set; }
        [Required]
        
        [Newtonsoft.Json.JsonConverter(typeof(SpellEntryUIListItemConvertor))]
        public List<SpellEntryUIListItem> Items { get; set; }
    }


    public partial class SpellEntryUIListItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Entries { get; set; }
    }


    public partial class SpellEntryItem
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public List<SpellEntry> Entries { get; set; }
    }
    public partial class SpellEntryTable
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public string Caption { get; set; }
        public List<string> ColLabels { get; set; }
        public List<string> ColStyles { get; set; }
        [Required]
        [Newtonsoft.Json.JsonConverter(typeof(SpellRowConverter))] 
        public List<RowClass> Rows { get; set; }
    }


    public partial class Spell
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public long Page { get; set; }
        public Srd? Srd { get; set; }
        public bool? BasicRules { get; set; }
        public long Level { get; set; }
        public string School { get; set; }
        public List<Time> Time { get; set; }
        public Range Range { get; set; }
        public Components Components { get; set; }
        public List<DurationElement> Duration { get; set; }

        public List<SpellEntry> Entries { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(ElementConverter<ScalingLevelDiceUnion>))]
        public List<ScalingLevelDiceUnion>? ScalingLevelDice { get; set; }
        public List<string> DamageInflict { get; set; }
        public List<string> SavingThrow { get; set; }
        public List<string> MiscTags { get; set; }
        public List<string> AreaTags { get; set; }
        public Classes Classes { get; set; }
        public List<Background> Backgrounds { get; set; }
        public List<EntriesHigherLevel> EntriesHigherLevel { get; set; }
        public List<Race> Races { get; set; }
        public Meta Meta { get; set; }
        public List<Background> EldritchInvocations { get; set; }
        public List<string> ConditionInflict { get; set; }
        public List<string> AffectsCreatureType { get; set; }
        public List<string> DamageResist { get; set; }
        public bool? HasFluffImages { get; set; }
        public List<string> SpellAttack { get; set; }
        public List<string> AbilityCheck { get; set; }
        public List<string> ConditionImmune { get; set; }
        public List<string> DamageVulnerable { get; set; }
        public List<string> DamageImmune { get; set; }
        public List<Source> OtherSources { get; set; }
        public List<Source> AdditionalSources { get; set; }
    }

    public partial class Source
    {
        public string SourceSource { get; set; }
        public long? Page { get; set; }
    }

    public partial class Background
    {
        public string Name { get; set; }
        public string Source { get; set; }
    }

    public partial class Classes
    {
        public List<Background> FromClassList { get; set; }
        public List<FromClassListVariant> FromClassListVariant { get; set; }
        public List<FromSubclass> FromSubclass { get; set; }
    }

    public partial class FromClassListVariant
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string DefinedInSource { get; set; }
    }

    public partial class FromSubclass
    {
        public Background Class { get; set; }
        public Subclass Subclass { get; set; }
    }

    public partial class Subclass
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string SubSubclass { get; set; }
    }

    public partial class Components
    {
        public bool? V { get; set; }
        public bool? S { get; set; }
        public MUnion? M { get; set; }
        public bool? R { get; set; }
    }

    public partial class MClass
    {
        public string Text { get; set; }
        public long? Cost { get; set; }
        public Srd? Consume { get; set; }
    }

    public partial class DurationElement
    {
        public string Type { get; set; }
        public DurationDuration Duration { get; set; }
        public bool? Concentration { get; set; }
        public List<string> Ends { get; set; }
    }

    public partial class DurationDuration
    {
        public string Type { get; set; }
        public long Amount { get; set; }
        public bool? UpTo { get; set; }
    }

    public partial class PurpleEntry
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<EntryEntryUnion> Entries { get; set; }
        public string Caption { get; set; }
        public List<string> ColLabels { get; set; }
        public List<string> ColStyles { get; set; }
        public List<List<RowElement>> Rows { get; set; }
        public List<Item> Items { get; set; }
        public string Source { get; set; }
        public long? Page { get; set; }
        public string Style { get; set; }
        public string By { get; set; }
    }

    public partial class FluffyEntry
    {
        public string Type { get; set; }
        public List<string> Items { get; set; }
    }

    public partial class EntriesHigherLevel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Entries { get; set; }
    }

    public partial class RowClass
    {
        public string Type { get; set; }
        public Roll Roll { get; set; }
    }

    public partial class Roll
    {
        public long? Exact { get; set; }
        public long? Min { get; set; }
        public long? Max { get; set; }
        public bool? Pad { get; set; }
    }

    public partial class Meta
    {
        public bool Ritual { get; set; }
    }

    public partial class Race
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string BaseName { get; set; }
        public string BaseSource { get; set; }
    }

    public partial class Range
    {
        public string Type { get; set; }
        public Distance Distance { get; set; }
    }

    public partial class Distance
    {
        public string Type { get; set; }
        public long? Amount { get; set; }
    }

    public partial class ScalingLevelDiceElement
    {
        public string Label { get; set; }
        public Scaling Scaling { get; set; }
    }

    public partial class Scaling
    {
        public string The1 { get; set; }
        public string The5 { get; set; }
        public string The11 { get; set; }
        public string The17 { get; set; }
    }

    public partial class Time
    {
        public long Number { get; set; }
        public string Unit { get; set; }
        public string Condition { get; set; }
    }

    public partial struct Srd
    {
        public bool? Bool;
        public string String;

        public static implicit operator Srd(bool Bool) => new Srd { Bool = Bool };
        public static implicit operator Srd(string String) => new Srd { String = String };
    }

    public partial struct MUnion
    {
        public MClass MClass;
        public string String;

        public static implicit operator MUnion(MClass MClass) => new MUnion { MClass = MClass };
        public static implicit operator MUnion(string String) => new MUnion { String = String };
    }

    public partial struct EntryEntryUnion
    {
        public FluffyEntry FluffyEntry;
        public string String;

        public static implicit operator EntryEntryUnion(FluffyEntry FluffyEntry) => new EntryEntryUnion { FluffyEntry = FluffyEntry };
        public static implicit operator EntryEntryUnion(string String) => new EntryEntryUnion { String = String };
    }

    public partial struct Item
    {
        public EntriesHigherLevel EntriesHigherLevel;
        public string String;

        public static implicit operator Item(EntriesHigherLevel EntriesHigherLevel) => new Item { EntriesHigherLevel = EntriesHigherLevel };
        public static implicit operator Item(string String) => new Item { String = String };
    }

    public partial struct RowElement
    {
        public RowClass RowClass;
        public string String;

        public static implicit operator RowElement(RowClass RowClass) => new RowElement { RowClass = RowClass };
        public static implicit operator RowElement(string String) => new RowElement { String = String };
    }

    [Newtonsoft.Json.JsonConverter(typeof(SpellEntryParser))]
    public partial class SpellEntry
    {
        public PurpleEntry PurpleEntry;
        public string String;

        public static implicit operator SpellEntry(PurpleEntry PurpleEntry) => new SpellEntry { PurpleEntry = PurpleEntry };
        public static implicit operator SpellEntry(string String) => new SpellEntry { String = String };
    }

    public partial struct ScalingLevelDiceUnion
    {
        public ScalingLevelDiceElement ScalingLevelDiceElement;
        public List<ScalingLevelDiceElement> ScalingLevelDiceElementArray;

        public static implicit operator ScalingLevelDiceUnion(ScalingLevelDiceElement ScalingLevelDiceElement) => new ScalingLevelDiceUnion { ScalingLevelDiceElement = ScalingLevelDiceElement };
        public static implicit operator ScalingLevelDiceUnion(List<ScalingLevelDiceElement> ScalingLevelDiceElementArray) => new ScalingLevelDiceUnion { ScalingLevelDiceElementArray = ScalingLevelDiceElementArray };
    }
}
