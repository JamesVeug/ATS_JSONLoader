﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ATS_JSONLoader;
using BepInEx;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace TinyJson
{
    // Really simple JSON parser in ~300 lines
    // - Attempts to parse JSON files with minimal GC allocation
    // - Nice and simple "[1,2,3]".FromJson<List<int>>() API
    // - Classes and structs can be parsed too!
    //      class Foo { public int Value; }
    //      "{\"Value\":10}".FromJson<Foo>()
    // - Can parse JSON without type information into Dictionary<string,object> and List<object> e.g.
    //      "[1,2,3]".FromJson<object>().GetType() == typeof(List<object>)
    //      "{\"Value\":10}".FromJson<object>().GetType() == typeof(Dictionary<string,object>)
    // - No JIT Emit support to support AOT compilation on iOS
    // - Attempts are made to NOT throw an exception if the JSON is corrupted or invalid: returns null instead.
    // - Only public fields and property setters on classes/structs will be written to
    //
    // Limitations:
    // - No JIT Emit support to parse structures quickly
    // - Limited to parsing <2GB JSON files (due to int.MaxValue)
    // - Parsing of abstract classes or interfaces is NOT supported and will throw an exception.
    public static class JSONParser
    {
        [ThreadStatic] static Stack<List<string>> splitArrayPool;
        [ThreadStatic] static StringBuilder stringBuilder;
        [ThreadStatic] static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache;
        [ThreadStatic] static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache;
        [ThreadStatic] static Dictionary<Type, FieldInfo[]> publicFieldInfoCache;
        [ThreadStatic] static Dictionary<Type, PropertyInfo[]> publicPropertyInfoCache;

        private static string LogPrefix = "";
        
        public static T FromFilePath<T>(this string filePath)
        {
            LogPrefix = filePath.Replace(Paths.PluginPath, "");
            if (LogPrefix.StartsWith("/") || LogPrefix.StartsWith("\\"))
                LogPrefix = LogPrefix.Substring(1);

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                throw new JsonException("File is empty! " + filePath);
            }
            else
            {
                Logging.VerboseLog($"Loading JSON from {filePath}\n{json}");
            }
            return FromJsonInternal<T>(json);
        }

        public static T FromJson<T>(this string json)
        {
            LogPrefix = "Unspecified Path";
            return FromJsonInternal<T>(json);
        }
        
        private static T FromJsonInternal<T>(this string json)
        {
            // Initialize, if needed, the ThreadStatic variables
            Initialize();

            //Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"')
                {
                    i = AppendUntilStringEnd(true, i, json);
                    continue;
                }
                if (c == '/' && (i + 1) != json.Length)
                {
                    if (json[i + 1] == '/')
                    {
                        i = SkipUntilLineEnd(i, json);
                        continue;
                    }
                }
                if (char.IsWhiteSpace(c))
                    continue;

                stringBuilder.Append(c);
            }

            //Parse the thing!
            return (T)ParseValue(typeof(T), stringBuilder.ToString());
        }

        private static void Initialize()
        {
            if (propertyInfoCache == null) propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (fieldInfoCache == null) fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            if (stringBuilder == null) stringBuilder = new StringBuilder();
            if (splitArrayPool == null) splitArrayPool = new Stack<List<string>>();
            if (publicFieldInfoCache == null) publicFieldInfoCache = new Dictionary<Type, FieldInfo[]>();
            if (publicPropertyInfoCache == null) publicPropertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
        }

        static int SkipUntilLineEnd(int startIdx, string json)
        {
            for (int i = startIdx + 2; i < json.Length; i++)
            {
                if (json[i] == '\n')
                {
                    return i + 1;
                }
            }
            return json.Length - 1;
        }

        static int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            stringBuilder.Append(json[startIdx]);
            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    if (appendEscapeCharacter)
                        stringBuilder.Append(json[i]);
                    stringBuilder.Append(json[i + 1]);
                    i++;//Skip next character as it is escaped
                }
                else if (json[i] == '"')
                {
                    stringBuilder.Append(json[i]);
                    return i;
                }
                else
                    stringBuilder.Append(json[i]);
            }
            return json.Length - 1;
        }

        //Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        static List<string> Split(string json, out int lastIndex)
        {
            List<string> splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();
            if (json.Length == 2)
            {
                lastIndex = -1;
                return splitArray;
            }

            int parseDepth = 0;
            stringBuilder.Length = 0;
            lastIndex = 1;
            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '"':
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case ',':
                    case ':':
                        lastIndex = i;
                        if (parseDepth == 0)
                        {
                            splitArray.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append(json[i]);
            }

            splitArray.Add(stringBuilder.ToString());

            return splitArray;
        }

        internal static object ParseValue(Type type, string json)
        {
            if (type == typeof(string))
            {
                if (json.Length <= 2)
                    return null;
                StringBuilder parseStringBuilder = new StringBuilder(json.Length);
                for (int i = 1; i < json.Length - 1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
                    {
                        int j = "\"\\nrtbf/".IndexOf(json[i + 1]);
                        if (j >= 0)
                        {
                            parseStringBuilder.Append("\"\\\n\r\t\b\f/"[j]);
                            ++i;
                            continue;
                        }
                        if (json[i + 1] == 'u' && i + 5 < json.Length - 1)
                        {
                            UInt32 c = 0;
                            if (UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c))
                            {
                                parseStringBuilder.Append((char)c);
                                i += 5;
                                continue;
                            }
                        }
                    }
                    parseStringBuilder.Append(json[i]);
                }
                return parseStringBuilder.ToString();
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var result = Convert.ChangeType(json, type.GetGenericArguments().First(), System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            if (type.IsPrimitive)
            {
                // Int/float/boolean
                var result = Convert.ChangeType(json, type, System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            if (type == typeof(decimal))
            {
                decimal result;
                decimal.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if (type == typeof(DateTime))
            {
                DateTime result;
                DateTime.TryParse(json.Replace("\"", ""), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);
                return result;
            }
            if (json == "null")
            {
                return null;
            }
            if (type.IsEnum)
            {
                if (json[0] == '"')
                    json = json.Substring(1, json.Length - 2);
                try
                {
                    return Enum.Parse(type, json, false);
                }
                catch(Exception ex)
                {
                    LogError(ex);
                    return 0;
                }
            }
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json, out _);
                Array newArray = Array.CreateInstance(elementType, elems.Count);
                for (int i = 0; i < elems.Count; i++)
                    newArray.SetValue(ParseValue(elementType, elems[i]), i);
                splitArrayPool.Push(elems);
                return newArray;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = type.GetGenericArguments()[0];
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json, out _);
                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });
                for (int i = 0; i < elems.Count; i++)
                    list.Add(ParseValue(listType, elems[i]));
                splitArrayPool.Push(elems);
                return list;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType, valueType;
                {
                    Type[] args = type.GetGenericArguments();
                    keyType = args[0];
                    valueType = args[1];
                }

                //Refuse to parse dictionary keys that aren't of type string
                if (keyType != typeof(string))
                    return null;
                //Must be a valid dictionary element
                if (json[0] != '{' || json[json.Length - 1] != '}')
                    return null;
                //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
                List<string> elems = Split(json, out _);
                if (elems.Count % 2 != 0)
                    return null;

                var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });
                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                        continue;
                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object val = ParseValue(valueType, elems[i + 1]);
                    dictionary[keyValue] = val;
                }
                return dictionary;
            }
            if (type == typeof(object))
            {
                return ParseAnonymousValue(json);
            }
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                return ParseObject(type, json);
            }

            return null;
        }

        static object ParseAnonymousValue(string json)
        {
            if (json.Length == 0)
                return null;
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                List<string> elems = Split(json, out _);
                if (elems.Count % 2 != 0)
                    return null;
                var dict = new Dictionary<string, object>(elems.Count / 2);
                for (int i = 0; i < elems.Count; i += 2)
                    dict[elems[i].Substring(1, elems[i].Length - 2)] = ParseAnonymousValue(elems[i + 1]);
                return dict;
            }
            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                List<string> items = Split(json, out _);
                var finalList = new List<object>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    finalList.Add(ParseAnonymousValue(items[i]));
                return finalList;
            }
            if (json[0] == '"' && json[json.Length - 1] == '"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }
            if (char.IsDigit(json[0]) || json[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }
            if (json == "true")
                return true;
            if (json == "false")
                return false;
            // handles json == "null" as well as invalid JSON
            return null;
        }

        static Dictionary<string, T> CreateMemberNameDictionary<T>(T[] members) where T : MemberInfo
        {
            Dictionary<string, T> nameToMember = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < members.Length; i++)
            {
                T member = members[i];
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;

                string name = member.Name;
                if (member.IsDefined(typeof(DataMemberAttribute), true))
                {
                    DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
                    if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                        name = dataMemberAttribute.Name;
                }

                nameToMember.Add(name.ToLower(), member);
            }

            return nameToMember;
        }

        static object ParseObject(Type type, string json)
        {
            object instance = FormatterServices.GetUninitializedObject(type);
            if (instance is IInitializable initializable)
            {
                // For LocaleFields to set themselves up since we can't use a constructor........
                initializable.Initialize();
            }

            //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json, out int lastIndex);
            if (elems.Count % 2 != 0)
            {
                // Get a substring of json that is left and right of lastIndex within 20 characters
                char[] splitCharacters = new char[] { '{', '}', '[', ']', ',' };
                
                int lastSplitIndex = lastIndex - 1;
                while (lastSplitIndex >= 0 && !splitCharacters.Contains(json[lastSplitIndex]))
                {
                    lastSplitIndex--;
                }
                
                string exampleString = "";
                if (lastSplitIndex < 0)
                {
                    int startIndex = Math.Max(0, lastIndex - 20);
                    int length = Math.Min(json.Length - startIndex, 40);
                    exampleString = json.Substring(startIndex, length);
                }
                else
                {
                    int startIndex = lastSplitIndex + 1;
                    int length = Math.Min(json.Length - startIndex, lastIndex);
                    exampleString = json.Substring(startIndex, length);
                }

                
                LogError($"Invalid JSON. Unexpected extra character found {json[lastIndex]} => {exampleString}");
                return instance;
            }

            Dictionary<string, FieldInfo> nameToField;
            Dictionary<string, PropertyInfo> nameToProperty;
            if (!fieldInfoCache.TryGetValue(type, out nameToField))
            {
                nameToField = CreateMemberNameDictionary(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
                fieldInfoCache.Add(type, nameToField);
            }
            if (!propertyInfoCache.TryGetValue(type, out nameToProperty))
            {
                nameToProperty = CreateMemberNameDictionary(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                propertyInfoCache.Add(type, nameToProperty);
            }
            List<MemberInfo> allMembers = null;

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;

                string key = elems[i].Substring(1, elems[i].Length - 2);
                string lowerKey = key.ToLower();
                string value = elems[i + 1];

                if (nameToField.TryGetValue(lowerKey, out FieldInfo fieldInfo) && (!fieldInfo.IsPrivate || fieldInfo.GetCustomAttribute<SerializeField>() != null))
                {
                    // Public fields or private ones with [SerializeField] set
                    SetField(fieldInfo, instance, value);
                }
                else if (nameToProperty.TryGetValue(lowerKey, out PropertyInfo propertyInfo))
                {
                    SetProperty(propertyInfo, instance, value);
                }
                else
                {
                    bool assigned = false;
                    foreach (KeyValuePair<string, FieldInfo> pair in nameToField)
                    {
                        FieldInfo info = pair.Value;
                        if (info.FieldType.GetInterfaces().Contains(typeof(IFlexibleField)))
                        {
                            object o = info.GetValue(instance);
                            if (o is IFlexibleField field)
                            {
                                if (field.ContainsKey(lowerKey))
                                {
                                    field.SetValueWithKey(lowerKey, (string)ParseValue(typeof(String), value));
                                    assigned = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!assigned)
                    {
                        string[] similarFields = FindSimilarFields(lowerKey, nameToField, nameToProperty);
                        if (similarFields != null && similarFields.Length > 0)
                        {
                            if (allMembers == null)
                            {
                                allMembers = new List<MemberInfo>();
                                allMembers.AddRange(nameToField.Values);
                                allMembers.AddRange(nameToProperty.Values);
                            }
                            
                            string[] unscaleNames = similarFields.Select((a) => allMembers.Find((b)=>b.Name.ToLower() == a).Name).ToArray();
                            string fields = string.Join(" or ", unscaleNames.Select((a) => "'" + a + "'"));
                            LogError($"{key} field not found for {type}. Did you mean {fields}?");
                        }
                        else
                        {
                            LogWarning($"{key} field not found for {type}. Could not find a field with a similar name. Are you sure you need this field?");
                        }
                    }
                }
            }

            return instance;
        }

        private static void LogWarning(string message)
        {
            Plugin.Log.LogWarning($"[{LogPrefix}] {message}");
        }

        private static void LogError(string message)
        {
            Plugin.Log.LogError($"[{LogPrefix}] {message}");
        }

        private static void LogError(Exception exception)
        {
            Plugin.Log.LogError($"[{LogPrefix}] {exception}");
        }

        private static string[] FindSimilarFields(string key, Dictionary<string,FieldInfo> nameToField, Dictionary<string,PropertyInfo> nameToProperty)
        {
            HashSet<string> allFields = new HashSet<string>();
            allFields.AddRange(nameToField.Keys);
            allFields.AddRange(nameToProperty.Keys);

            return ImportExportUtils.FindSimilarStrings(key, allFields);
        }

        private static void SetField(FieldInfo info, object o, string v)
        {
            if (info.FieldType.GetInterfaces().Contains(typeof(IFlexibleField)))
            {
                object fieldValue = info.GetValue(o);
                if (fieldValue == null)
                {
                    LogError($"{info.Name} field is null! Type: {info.FieldType} o:{o} instance:{o}");
                }
                else if (fieldValue is IFlexibleField flexibleField)
                {
                    flexibleField.SetValueWithKey(info.Name, (string)ParseValue(typeof(String), v));
                }
            }
            else
            {
                info.SetValue(o, ParseValue(info.FieldType, v));
            }
        }

        private static void SetProperty(PropertyInfo info, object o, string v)
        {
            info.SetValue(o, ParseValue(info.PropertyType, v), null);
        }

        public static string ToJSON<T>(T t)
        {
            Initialize();
            return ToJSONInternal(typeof(T), t, "");
        }

        private static string ToJSONInternal(Type type, object t, string prefix)
        {
            try
            {
                if (type == typeof(string))
                {
                    string fieldVal = (string)t;
                    if (fieldVal != null)
                    {
                        return "\"" + fieldVal + "\"";
                    }
                    else
                    {
                        return "\"\"";
                    }
                }
                else if (type.IsArray)
                {
                    if (t == null)
                    {
                        return "null";
                    }
                    return JsonInternalArray(t, prefix);
                }
                else if (type == typeof(int?))
                {
                    int? fieldVal = (int?)t;
                    if (fieldVal.HasValue)
                        return fieldVal.Value.ToString();
                    return "0";
                }
                else if (type == typeof(bool?))
                {
                    bool? fieldVal = (bool?)t;
                    if (fieldVal.HasValue)
                        return fieldVal.Value ? "true" : "false";
                    return "false";
                }
                else if (type == typeof(bool))
                {
                    return (bool)t ? "true" : "false";
                }
                else if (type == typeof(int) || type == typeof(long))
                {
                    return t.ToString();
                }
                else if (type == typeof(float))
                {
                    return $"{t}";
                }
                else if (type == typeof(float?))
                {
                    float? fieldVal = (float?)t;
                    if (fieldVal.HasValue)
                        return fieldVal.Value.ToString();
                    return "0.0";
                }
                else if (type == typeof(Dictionary<string, string>))
                {
                    Dictionary<string, string> fieldVal = (Dictionary<string, string>)t;
                    if (fieldVal != null && fieldVal.Count > 0)
                    {
                        string s = "{";
                        int index = 0;
                        foreach (KeyValuePair<string, string> pair in fieldVal)
                        {
                            if (index++ > 0)
                            {
                                s += $",\n\t{prefix}\"{pair.Key}\": \"{pair.Value}\"";
                            }
                            else
                            {
                                s += $"\n\t{prefix}\"{pair.Key}\": \"{pair.Value}\"";
                            }
                        }

                        if (index > 0)
                        {
                            return $"{s}\n{prefix}}}";
                        }

                        return s + "}";
                    }
                    else
                    {
                        return "{}";
                    }
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    IList fieldVal = (IList)t;
                    if (fieldVal != null && fieldVal.Count > 0)
                    {
                        Type subType = fieldVal.GetType().GetGenericArguments().Single();
                        string join = "";
                        string subPrefix = prefix + "\t";
                        for (int i = 0; i < fieldVal.Count; i++)
                        {
                            object o = fieldVal[i];
                            join += "\n" + subPrefix + ToJSONInternal(subType, o, subPrefix);
                            if (i < fieldVal.Count - 1)
                                join += ",";
                        }

                        // [
                        // "Hello",
                        // ]
                        return $"[{join}\n{prefix}]";
                    }
                    else
                    {
                        return "[]";
                    }
                }
                else if (!type.IsValueType)
                {
                    if (t == null)
                    {
                        return "null";
                    }
                 
                    if (!publicFieldInfoCache.TryGetValue(type, out FieldInfo[] PUBLIC_FIELD_INFOS))
                    {
                        PUBLIC_FIELD_INFOS = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        
                        // Order fields by base order in hierarchy and order in that class
                        PUBLIC_FIELD_INFOS = PUBLIC_FIELD_INFOS.OrderBy(OrderFieldsByHierarchy).ToArray();
                        
                        // LogWarning($"{type.Name} Fields: {PUBLIC_FIELD_INFOS.Length} {string.Join(", ", PUBLIC_FIELD_INFOS.Select((a) => a.Name))}");
                        
                        publicFieldInfoCache[type] = PUBLIC_FIELD_INFOS;
                    } 
                    if (!publicPropertyInfoCache.TryGetValue(type, out PropertyInfo[] PUBLIC_PROPERTY_INFOS))
                    {
                        PUBLIC_PROPERTY_INFOS = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                        publicPropertyInfoCache[type] = PUBLIC_PROPERTY_INFOS;
                    }

                    string s = "{";
                    int index = 0;
                    string subPrefix = prefix + "\t";
                    foreach (FieldInfo fieldInfo in PUBLIC_FIELD_INFOS)
                    {
                        if (fieldInfo.IsPrivate && fieldInfo.GetAttribute<SerializeField>() == null)
                            continue;

                        if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(IFlexibleField)))
                        {
                            if (index > 0)
                            {
                                s += ",";
                            }
                            s += ((IFlexibleField)fieldInfo.GetValue(t)).ToJSON(subPrefix);
                            index++;
                        }
                        else
                        {
                            string value = ToJSONInternal(fieldInfo.FieldType, fieldInfo.GetValue(t), subPrefix);
                            if (index++ > 0)
                            {
                                s += ",";
                            }

                            s += $"\n{subPrefix}\"{fieldInfo.Name}\": {value}";
                        }
                    }
                    foreach (PropertyInfo property in PUBLIC_PROPERTY_INFOS)
                    {
                        LogWarning($"Property {property.Name}");
                        string value = ToJSONInternal(property.PropertyType, property.GetValue(t), subPrefix);
                        if (index++ > 0)
                        {
                            s += ",";
                        }
                        s += $"\n{subPrefix}\"{property.Name}\": {value}";
                    }

                    if (index > 0)
                    {
                        return s + $"\n{prefix}" + "}";
                    }

                    return s + prefix + "}";
                }
            }
            catch (Exception e)
            {
                LogError($"Something went wrong while serializing JSON type: {type} value: {t}");
                LogError(e);
                throw;
            }

            throw new NotImplementedException($"Type not supported for JSON serialization {type}");
        }

        public static int OrderFieldsByHierarchy(FieldInfo a)
        {
            // Get depth in hierarchy
            Type baseType = a.DeclaringType;
            while (baseType.BaseType != null)
            {
                baseType = baseType.BaseType;
            }

            int depth = 0;
            Type currentType = a.DeclaringType;
            while (currentType != null)
            {
                depth++;
                currentType = currentType.BaseType;
            }

            int indexOf = a.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).IndexOf(a);
            return depth * 1000 + indexOf;
        }

        private static string JsonInternalArray(object t, string prefix)
        {
            Array array = (Array)t;
            if (array != null)
            {
                Type elementType = array.GetType().GetElementType();
                        
                string s = "";
                s += "[";
                for (var j = 0; j < array.Length; j++)
                {
                    s += ToJSONInternal(elementType, array.GetValue(j), prefix + "\t");
                    if (j < array.Length - 1)
                        s += ",\n" + prefix + "\t";
                }

                if (array.Length > 1)
                {
                    s += "\n" + prefix;
                }
                s += "]";
                return s;
            }
            else
            {
                return "[]";
            }
        }
    }
}
