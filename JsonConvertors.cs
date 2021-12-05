using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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
                    var mergedRows = token.Where(c => c.Type == JTokenType.Array).Select(c =>  c.ToObject<RowClass>());
                    result.AddRange(mergedRows);
                }
                else
                {
                    result = token.ToObject<List<RowClass>>();
                }
                return result;
            }
            return new List<string>();
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

                var tableProps = GetRequiredAttribues(typeof(SpellEntryTable));
                if (keys.Count(k => tableProps.Contains(k)) == tableProps.Count())
                {
                    var spellEntryList = token.ToObject<SpellEntryTable>();
                    var parsedRows = string.Join(" ", spellEntryList.Rows.Select(c=>c.Roll.Exact.HasValue ? $"[Roll {c.Roll.Exact}]" : $"[Roll {c.Roll.Min} - {c.Roll.Min}]"));
                    var SpellEntry = new SpellEntry()
                    {
                        String = parsedRows
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
            else if (reader.ValueType == typeof(SpellEntryUIList))
            {
                return new SpellEntry()
                {
                    String = string.Join("& ", token.ToObject<SpellEntryUIList>().Items.Select(c => string.Join("& ", c.Entries.Select(d => string.Join("& ", d.Select(c => c)))).ToArray()))
                };
            }
            else if (reader.ValueType == typeof(SpellEntryItem))
            {
                return new SpellEntry()
                {
                    String = string.Join("& ", token.ToObject<SpellEntryItem>().Entries.Select(c => c.String))
                };
            }
            else if (objectType == typeof(SpellEntryTable))
            {
                return new SpellEntry()
                {
                    //String = string.Join("& ", token.ToObject<SpellEntryTable>().Rows.Select(c => string.Join("&", c.Select(c => string.Join("& ", c.Select(c => c))))))
                };
            }
            return "";

        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
