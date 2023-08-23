//https://raw.githubusercontent.com/TheGiddyLimit/5etools-utils/master/schema/ua/spells/spells.json

// Enums
public enum SpellSchool 
{
  Abjuration,
  Conjuration,
  Divination,
  Enchantment,
  Evocation,
  Illusion,
  Necromancy,
  Transmutation
}

public enum SpellAttackType
{
  M, // Melee
  R, // Ranged
  O // Other/Unknown
}

public enum CreatureType
{
  Aberration,
  Beast,
  Celestial,
  Construct,
  Dragon,
  Elemental,
  Fey,
  Fiend,
  Giant,
  Humanoid,
  Monstrosity,
  Ooze,
  Plant,
  Undead
}

public enum Ability
{
  Strength,
  Constitution,
  Dexterity,
  Intelligence,
  Wisdom,
  Charisma
}

public enum SpellRangeType
{
  Special,
  Point,
  Line,
  Cube,
  Cone,
  Radius,
  Sphere,
  Hemisphere,
  Cylinder  
}

public enum SpellRangeDistanceType
{
  Feet,
  Yards,
  Miles,
  Self,
  Touch,
  Unlimited,
  Plane,
  Sight
}

public enum SpellTimeUnit
{
  Action,
  Bonus,
  Reaction,
  Round,
  Minute,
  Hour  
}

public enum SpellDurationType
{
  Instant,
  Timed,
  Permanent,
  Special
}

public enum SpellDurationDurationType
{
  Hour,
  Minute,
  Turn,
  Round,
  Week,
  Day,
  Year
}

public enum AreaTag
{
  ST, // Single Target
  MT, // Multiple Targets
  R, // Circle
  N, // Cone
  C, // Cube
  Y, // Cylinder
  H, // Hemisphere
  L, // Line
  S, // Sphere
  Q, // Square
  W // Wall  
}

public enum MiscTag
{
  AAD, // Additional Attack Damage
  DFT, // Difficult Terrain
  FMV, // Forced Movement
  HL, // Healing
  LGT, // Creates Light
  LGTS, // Creates Sunlight
  MAC, // Modifies AC
  OBJ, // Affects Objects
  OBS, // Obscures Vision
  PRM, // Permanent Effects
  PS, // Plane Shifting
  RO, // Rollable Effects
  SCL, // Scaling Effects
  SMN, // Summons Creature
  SGT, // Requires Sight
  THP, // Grants Temporary Hit Points
  TP, // Teleportation
  UBA // Uses Bonus Action
} 

public enum SpellEndType
{
  Dispel,
  Trigger,
  Discharge
}

// Interfaces
public interface ISpellComponent {}

// Classes
public class SpellData 
{
  public string Name { get; set; }
  public int Level { get; set; }
  public SpellSchool School { get; set; }
  public SpellMeta Meta { get; set; }
  public List<SpellTime> Time { get; set; }
  public SpellRange Range { get; set; }
  public SpellComponents Components { get; set; }
  public List<SpellDuration> Duration { get; set; }
  public List<Entry> Entries { get; set; }
  public List<Entry> EntriesHigherLevel { get; set; }
  public Source Source { get; set; }
  public string Page { get; set; }
  public List<Source> AdditionalSources { get; set; }
  public List<OtherSource> OtherSources { get; set; }
  public List<DamageType> DamageInflict { get; set; }
  public List<DamageType> DamageResist { get; set; }
  public List<DamageType> DamageImmune { get; set; }
  public List<DamageType> DamageVulnerable { get; set; }
  public List<string> ConditionInflict { get; set; }
  public List<string> ConditionImmune { get; set; }
  public List<string> SavingThrow { get; set; }
  public List<Ability> AbilityCheck { get; set; }
  public List<SpellAttackType> SpellAttack { get; set; }
  public List<string> AreaTags { get; set; } 
  public List<string> MiscTags { get; set; }
  public List<CreatureType> AffectsCreatureType { get; set; }
  public bool? Srd { get; set; }
  public bool? BasicRules { get; set; }
  public bool? Legacy { get; set; }
  public List<ScalingLevelDiceItem> ScalingLevelDice { get; set; }
  public bool HasFluff { get; set; }
  public bool HasFluffImages { get; set; }
  public List<SpellGroup> Groups { get; set; }
}

public class SpellMeta
{
  public bool Ritual { get; set; }
  public bool Technomagic { get; set; }
}

public class SpellTime
{
  public int Number { get; set; }
  public SpellTimeUnit Unit { get; set; } 
  public string Condition { get; set; }
}

public class SpellRange
{
  public SpellRangeType Type { get; set; }
  public SpellRangeDistance Distance { get; set; }
}

public class SpellRangeDistance
{
  public SpellRangeDistanceType Type { get; set; }
  public int Amount { get; set; }
}

public class SpellComponents
{
  public bool V { get; set; }
  public bool S { get; set; }
  public ISpellComponent M { get; set; }
  public bool R { get; set; }
}

public class SpellMaterialComponent : ISpellComponent
{
  public string Text { get; set; }
  public int? Cost { get; set; }
  public bool? Consume { get; set; } // Can also be "optional" string
}

public class SpellDuration
{
  public SpellDurationType Type { get; set; }
  public SpellDurationDetails Duration { get; set; }
  public bool? Concentration { get; set; }
  public List<SpellEndType> Ends { get; set; } 
  public string Condition { get; set; }
}

public class SpellDurationDetails 
{
  public SpellDurationDurationType Type { get; set; }
  public int Amount { get; set; }
  public bool UpTo { get; set; }
}

public class SpellGroup
{
  public string Name { get; set; }
  public Source Source { get; set; }
}

public class ScalingLevelDiceItem
{
  public string Label { get; set; }
  public Dictionary<string, string> Scaling { get; set; } 
}
