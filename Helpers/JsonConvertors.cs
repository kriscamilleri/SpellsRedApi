using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpellsRedApi.Models.Giddy;

namespace SpellsRedApi
{


    public class ElementConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    public class SpellRowConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>));
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                List<RowClass> result = new List<RowClass>();
                if (token.Any(c => c.Type == JTokenType.Array))
                {
                    List<RowClass> fromStringChildren = token
                        .Where(c => c.Children().Any(d => d.Type == JTokenType.String))
                        .Select(c => c.Children()
                            .Where(d => d.Type == JTokenType.String)
                                .Select(d => d.ToObject<string>() ?? "")
                                .ToList())
                        .ToList()
                        //.Select(c=> string.Join(" | ", c))
                        .Select(c=> new RowClass()
                        {
                            TextArray = c
                        })
                        .ToList();
                    var objectChildren = token
                        .Where(c => c.Children().Any(d => d.Type == JTokenType.Object))
                        .SelectMany(c => c.Children()
                            .Where(d => d.Type == JTokenType.Object)
                                .Select(d => d.Type == JTokenType.String ? new RowClass() { Text = d.ToString() } :  d.ToObject<RowClass>() ?? new RowClass())
                                .ToList())
                        .ToList();


                    if(fromStringChildren.Count > 0)
                    {
                        result.AddRange(fromStringChildren);
                    }
                    if (objectChildren.Count > 0)
                    {
                        result.AddRange(objectChildren);
                    }
                }
                else
                {
                    result = token.ToObject<List<RowClass>>();
                }
                return result;
            }
            return new List<string>();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class SpellEntryParser : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(string)
                || objectType == typeof(SpellEntryUIList)
                || objectType == typeof(SpellEntryItem)
                || objectType == typeof(SpellEntryTable));
        }

        private string[] GetRequiredAttribues(Type type)
        {
            var attributes = type.GetProperties().Where(p => p.GetCustomAttributes(false).Any(c => c.GetType().Name == "RequiredAttribute")).Select(c => c.Name.ToLower());
            return attributes.ToArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (reader.ValueType == null)
            {
                var jObject = token.ToObject<JObject>();
                var children = jObject.Children().Select(c => new { Key = c.Path, Value = c.Values() });
                var keys = children.Select(c => c.Key);

                var listProps = GetRequiredAttribues(typeof(SpellEntryUIList));
                if (keys.Count(k => listProps.Contains(k)) == listProps.Count())
                {
                    var spellEntryList = token.ToObject<SpellEntryUIList>();
                    var SpellEntry = new SpellEntry()
                    {
                        String = string.Join(" ", spellEntryList.Items.Select(c => string.Join("& ", c.Entries)))
                    };
                    return SpellEntry;
                }
                var ItemProps = GetRequiredAttribues(typeof(SpellEntryItem));
                if (keys.Count(k => ItemProps.Contains(k)) == ItemProps.Count())
                {
                    var spellEntryList = token.ToObject<SpellEntryItem>();
                    var SpellEntry = new SpellEntry()
                    {
                        String = $"{spellEntryList?.Name ?? ""}: {string.Join(" ", spellEntryList?.Entries.Select(c => string.Join("& ", c.String)) ?? new List<string>())}"
                    };
                    return SpellEntry;
                 }

                var tableProps = GetRequiredAttribues(typeof(SpellEntryTable));
                if (keys.Count(k => tableProps.Contains(k)) == tableProps.Count())
                {
                    var spellEntryList = token.ToObject<SpellEntryTable>();
                    string parsedRow = "";
                    foreach(var row in spellEntryList?.Rows ?? new List<RowClass>())
                    {
                        if(row.TextArray != null && row.TextArray.Count() > 0)
                        {
                            var joinedText = string.Join(" || ", row.TextArray);
                            parsedRow += $"<br> {joinedText} ";
                        }
                        if(row.Roll != null)
                        {
                            if (row.Roll.Exact.HasValue)
                            {
                                parsedRow += $"<br> [Roll { row.Roll.Exact}] ";
                            }
                            if (row.Roll.Min.HasValue && row.Roll.Max.HasValue)
                            {
                                parsedRow += $"[Roll {row.Roll.Min} - {row.Roll.Min}]";
                            }
                        }
                        if (row.Text != null && row.Text.Length > 0)
                        {
                            parsedRow += row.Text;
                        }
                    }
                    var SpellEntry = new SpellEntry()
                    {
                        String = parsedRow
                    };
                    return SpellEntry;
                }

                return new SpellEntry()
                {
                    //String = string.Join("& ", spellEntries.Items.Select(c => string.Join("& ", c.Entries.Select(d => string.Join("& ", d.Select(c => c)))).ToArray()))
                };

            }

            if (reader.ValueType == typeof(string))
            {
                return new SpellEntry()
                {
                    String = token.ToString()
                };
            }
           
            return "";
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class SpellEntryUIListItemConvertor : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(string)
                || objectType == typeof(SpellEntryUIListItem));
        }

        private string[] GetRequiredAttribues(Type type)
        {
            var attributes = type.GetProperties().Where(p => p.GetCustomAttributes(false).Any(c => c.GetType().Name == "RequiredAttribute")).Select(c => c.Name.ToLower());
            return attributes.ToArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (reader.ValueType == typeof(string))
            {
                return new List<SpellEntryUIListItem> {
                    new SpellEntryUIListItem()
                    {
                        Type = "list-item",
                        Entries = new List<string>() { reader.Value.ToString() }
                    }
                };
            }
            else if (token.Type == JTokenType.Array)
            {
                var jArray = token.ToObject<JArray>();
                if (jArray.Children().Any(c => c.Type == JTokenType.String))
                {

                    return new List<SpellEntryUIListItem> {
                    new SpellEntryUIListItem()
                    {
                        Type = "list-item",
                        Entries =  jArray.Children().Select(c => c.ToString()).ToList()
                        }
                    };
                }
                else
                {
                    //var children = jArray.Children().Select(c => new { Key = c.Path, Value = c.Select(d=> d.Values()) });
                    var children = jArray.Children().Select(c => c.ToObject<SpellEntryItem>());
                    var keys = children.Select(c => c.Type);
                    var tableProps = GetRequiredAttribues(typeof(SpellEntryItem));
                    var result = 
                     new List<SpellEntryUIListItem> {
                                    new SpellEntryUIListItem()
                                    {
                                        Type = "list-item",
                                        Entries =  children.Select(c=> string.Join(", ",c.Entries.Select(d=> d.String))).ToList()
                                    }
                                 };
                    return result;
                }
             
                return new List<SpellEntryUIListItem> {
                    new SpellEntryUIListItem()
                    {
                        Type = "list-item",
                        Entries = token.Values<string>().ToList()
                        }
                    };
            }
            return token.ToObject<SpellEntryUIListItem>();

        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
