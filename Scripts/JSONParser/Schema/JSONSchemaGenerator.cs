using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ATS_JSONLoader;
using Newtonsoft.Json;
using TinyJson;

public static class JSONSchemaGenerator
{
    public static void GenerateAndExport()
    {
        // Find all classes with the [GenerateSchema] attribute
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(GenerateSchemaAttribute), false).Any());
        Plugin.Log.LogInfo($"Found {types.Count()} classes with the GenerateSchema attribute");
        
        // Generate the schema for each class
        foreach (var type in types)
        {
            Plugin.Log.LogInfo($"Exporting schema for {type.Name}");
            Dictionary<string, object> schema = GenerateSchemaHighLevelType(type);
            
            string json = JsonConvert.SerializeObject(schema, Formatting.Indented);
            string path = Path.Combine(Plugin.ExportDirectory, "Schemas", type.Name + ".json");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            
            File.WriteAllText(path, json);
        }
        
        Plugin.Log.LogInfo("Exported all schemas");
    }

    private static Dictionary<string, object> GenerateSchemaHighLevelType(Type type)
    {
        Dictionary<string,object> generateType = GenerateSchemaForType(type);
        
        Dictionary<string, object> schema = new Dictionary<string, object>();
        schema["$schema"] = "http://json-schema.org/draft-04/schema#";
        schema["id"] = "https://github.com/JamesVeug/ATS_JSONLoader";
        foreach (KeyValuePair<string,object> pair in generateType)
        {
            schema[pair.Key] = pair.Value;
        }
        
        return schema;
    }
    
    private static Dictionary<string, object> GenerateSchemaForType(Type type)
    {
        Dictionary<string, object> schema = new Dictionary<string, object>();
        schema["type"] = GetFieldType(type);
        // schema["additionalProperties"] = false; // NOTE: What is this?
        
        GenerateSchemaAttribute attribute = type.GetCustomAttribute<GenerateSchemaAttribute>();
        if (attribute != null)
        {
            schema["title"] = attribute.GetTitle();
            schema["description"] = attribute.GetDescription();
        }
        
        FieldInfo[] fieldInfos = type.GetFields().Where(a=>a.GetCustomAttribute<SchemaFieldAttribute>() != null).ToArray();
        string[] required = fieldInfos.Where(a=>a.GetCustomAttribute<SchemaFieldAttribute>().IsRequired()).Select(a=>a.Name).ToArray();
        if (required.Length > 0)
        {
            schema["required"] = required;
        }

        if (fieldInfos.Length > 0)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            schema["properties"] = properties;

            foreach (FieldInfo field in fieldInfos)
            {
                Plugin.Log.LogInfo($"Generating schema for {field.Name}");
                properties[field.Name] = GenerateSchemaForField(field);
            }
        }

        return schema;
    }

    private static Dictionary<string, object> GenerateSchemaForField(FieldInfo fieldInfo)
    {
        Dictionary<string, object> fieldSchema = new Dictionary<string, object>();

        Type type = fieldInfo.FieldType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // Plugin.Log.LogInfo("Nullable type " + type + " to " + type.GetGenericArguments()[0].Name);
            type = Nullable.GetUnderlyingType(type);
        }
        
        SchemaFieldAttribute schemaFieldAttribute = fieldInfo.GetCustomAttribute<SchemaFieldAttribute>();
        object defaultValue = schemaFieldAttribute.GetDefaultValue();
        bool isEnum = schemaFieldAttribute.GetEnum().Length > 0;
        if (isEnum || defaultValue != null)
        {
            fieldSchema["default"] = isEnum ? defaultValue.ToString() : defaultValue;
        }

        string description = schemaFieldAttribute.GetDescription();
        if(TryGetAttributes(schemaFieldAttribute, out SchemaHelpURLAttribute[] helpURLAttributes))
        {
            description += "\n\nFor more information, see:";
            foreach (SchemaHelpURLAttribute helpURLAttribute in helpURLAttributes)
            {
                description += $"\n{helpURLAttribute.URL}";
            }
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            fieldSchema["description"] = description;
        }

        if (!string.IsNullOrEmpty(schemaFieldAttribute.GetPattern()))
        {
            fieldSchema["pattern"] = schemaFieldAttribute.GetPattern();
        }

        if (schemaFieldAttribute.GetMinLength() != 0)
        {
            fieldSchema["minLength"] = schemaFieldAttribute.GetMinLength();
        }

        AddFieldType(type, schemaFieldAttribute, fieldSchema);

        return fieldSchema;
    }

    private static bool TryGetAttribute<T>(SchemaFieldAttribute baseAttribute, out T foundAttribute) where T : Attribute
    {
        foundAttribute = baseAttribute.GetType().GetCustomAttribute<T>();
        return foundAttribute != null;
    }
    
    private static bool TryGetAttributes<T>(SchemaFieldAttribute baseAttribute, out T[] t) where T : Attribute
    {
        t = baseAttribute.GetType().GetCustomAttributes<T>().ToArray();
        return t.Length > 0;
    }

    private static void AddFieldType(Type type, SchemaFieldAttribute fieldAttribute, Dictionary<string, object> schema)
    {
        if (type.IsArray)
        {
            Dictionary<string, object> items = new Dictionary<string, object>();
            AddFieldType(type.GetElementType(), fieldAttribute, items);
            
            schema["type"] = "array";
            schema["items"] = items;
            return;
        }
        
        bool isEnum = fieldAttribute.GetEnum().Length > 0;
        if (isEnum)
        {
            Type enumType = fieldAttribute.GetDefaultValue().GetType();
            
            Dictionary<string, object> enumOptions = new Dictionary<string, object>();
            enumOptions["type"] = "string";
            enumOptions["title"] = "Predefined " + enumType.Name;
            // enumOptions["description"] = "Options added in the base game that you can reuse.";
            enumOptions["enum"] = fieldAttribute.GetEnum();

            Dictionary<string, object> manualEnter = CustomEnumString(enumType);

            List<Dictionary<string,object>> options = new List<Dictionary<string, object>>();
            if (fieldAttribute.GetEnum().Length <= 100)
            {
                options.Add(enumOptions);
            }
            else
            {
                Plugin.Log.LogWarning($"Enum for {type.Name} has more than 100 options, skipping predefined enum!");
            }
            options.Add(manualEnter);
            
            schema["anyOf"] = options;
        }
        else
        {
            Dictionary<string, object> typeDetails = GenerateSchemaForType(type);
            foreach (KeyValuePair<string,object> pair in typeDetails)
            {
                schema[pair.Key] = pair.Value;
            }
        }
    }

    private static Dictionary<string, object> CustomEnumString(Type type)
    {
        Dictionary<string, object> schema = new Dictionary<string, object>();
        schema["type"] = "string";
        schema["title"] = "Mod-Added " + type.Name;
        // schema["description"] = "Format: [GUID].[" + type.Name + " Name]";
        
        return schema;
    }

    private static string GetFieldType(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }
        if (type == typeof(int))
        {
            return "integer";
        }
        if (type == typeof(float))
        {
            return "number";
        }
        if (type == typeof(bool))
        {
            return "boolean";
        }
        if (type == typeof(DateTime))
        {
            return "string";
        }
        if (type.IsEnum)
        {
            return "string";
        }
        if (type.IsArray)
        {
            return "array";
        }

        if (type.GetInterfaces().Contains(typeof(IFlexibleField)))
        {
            return "string";
        }
        
        return "object";
    }
}