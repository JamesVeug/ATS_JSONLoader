using System;
using System.Collections.Generic;
using ATS_JSONLoader;
using UnityEngine;

namespace TinyJson;

[Serializable]
public class LocalizableField : IFlexibleField
{
    public string EnglishValue
    {
        get
        {
            if (rows.TryGetValue(SystemLanguage.English, out var englishValue))
            {
                return englishValue;
            }

            Plugin.Log.LogError($"Field has not been initialized {englishFieldName}!");
            return englishFieldName;
        }
    }

    public Dictionary<SystemLanguage, string> rows;

    public string englishFieldName;
    public string englishFieldNameLower;

    public LocalizableField(string EnglishFieldName)
    {
        rows = new Dictionary<SystemLanguage, string>();
        englishFieldName = EnglishFieldName;
        englishFieldNameLower = EnglishFieldName.ToLower();
    }

    public void Initialize(string englishValue)
    {
        rows[SystemLanguage.English] = englishValue;
    }

    public bool ContainsKey(string key)
    {
        return key.StartsWith(englishFieldNameLower);
    }

    public void SetValueWithKey(string key, string value)
    {
        if (key == englishFieldName)
        {
            rows[SystemLanguage.English] = value;
            return;
        }
        
        int indexOf = key.LastIndexOf("_");
        string languageCode = key.Substring(indexOf + 1);
        
        SystemLanguage language = CodeToLanguage(languageCode);
        rows[language] = value;
    }
    
    public void SetValue(SystemLanguage language, string value)
    {
        rows[language] = value;
    }
    
    public void SetValueWithLanguageCode(string languageCode, string value)
    {
        SystemLanguage language = CodeToLanguage(languageCode);
        rows[language] = value;
    }

    public static string LanguageToCode(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.English:
                return "en";
            case SystemLanguage.Afrikaans:
                return "af";
            case SystemLanguage.Arabic:
                return "ar";
            case SystemLanguage.Basque:
                return "eu";
            case SystemLanguage.Belarusian:
                return "be";
            case SystemLanguage.Bulgarian:
                return "bg";
            case SystemLanguage.Catalan:
                return "ca";
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
                return "zh-CN";
            case SystemLanguage.ChineseTraditional:
                return "zh-CT"; // Normally this is TW but ATS uses CT for traditional
            case SystemLanguage.Czech:
                return "cs";
            case SystemLanguage.Danish:
                return "da";
            case SystemLanguage.Dutch:
                return "nl";
            case SystemLanguage.Estonian:
                return "et";
            case SystemLanguage.Faroese:
                return "fo";
            case SystemLanguage.Finnish:
                return "fi";
            case SystemLanguage.French:
                return "fr";
            case SystemLanguage.German:
                return "de";
            case SystemLanguage.Greek:
                return "el";
            case SystemLanguage.Hebrew:
                return "he";
            case SystemLanguage.Icelandic:
                return "is";
            case SystemLanguage.Indonesian:
                return "id";
            case SystemLanguage.Italian:
                return "it";
            case SystemLanguage.Japanese:
                return "ja";
            case SystemLanguage.Korean:
                return "ko";
            case SystemLanguage.Latvian:
                return "lv";
            case SystemLanguage.Lithuanian:
                return "lt";
            case SystemLanguage.Norwegian:
                return "no";
            case SystemLanguage.Polish:
                return "pl";
            case SystemLanguage.Portuguese:
                return "pt";
            case SystemLanguage.Romanian:
                return "ro";
            case SystemLanguage.Russian:
                return "ru";
            case SystemLanguage.SerboCroatian:
                return "sh";
            case SystemLanguage.Slovak:
                return "sk";
            case SystemLanguage.Slovenian:
                return "sl";
            case SystemLanguage.Spanish:
                return "es";
            case SystemLanguage.Swedish:
                return "sv";
            case SystemLanguage.Thai:
                return "th";
            case SystemLanguage.Turkish:
                return "tr";
            case SystemLanguage.Ukrainian:
                return "uk";
            case SystemLanguage.Vietnamese:
                return "vi";
            default:
                return "en"; //Default to English if the language is not in the list
        }
    }

    public static SystemLanguage CodeToLanguage(string code)
    {
        switch (code)
        {
            case "en":
                return SystemLanguage.English;
            case "af":
                return SystemLanguage.Afrikaans;
            case "ar":
                return SystemLanguage.Arabic;
            case "eu":
                return SystemLanguage.Basque;
            case "be":
                return SystemLanguage.Belarusian;
            case "bg":
                return SystemLanguage.Bulgarian;
            case "ca":
                return SystemLanguage.Catalan;
            case "zh-CN":
                return SystemLanguage.ChineseSimplified;
            case "zh-CT":
                return SystemLanguage.ChineseTraditional;
            case "cs":
                return SystemLanguage.Czech;
            case "da":
                return SystemLanguage.Danish;
            case "nl":
                return SystemLanguage.Dutch;
            case "et":
                return SystemLanguage.Estonian;
            case "fo":
                return SystemLanguage.Faroese;
            case "fi":
                return SystemLanguage.Finnish;
            case "fr":
                return SystemLanguage.French;
            case "de":
                return SystemLanguage.German;
            case "el":
                return SystemLanguage.Greek;
            case "he":
                return SystemLanguage.Hebrew;
            case "is":
                return SystemLanguage.Icelandic;
            case "id":
                return SystemLanguage.Indonesian;
            case "it":
                return SystemLanguage.Italian;
            case "ja":
                return SystemLanguage.Japanese;
            case "ko":
                return SystemLanguage.Korean;
            case "lv":
                return SystemLanguage.Latvian;
            case "lt":
                return SystemLanguage.Lithuanian;
            case "no":
                return SystemLanguage.Norwegian;
            case "pl":
                return SystemLanguage.Polish;
            case "pt":
                return SystemLanguage.Portuguese;
            case "ro":
                return SystemLanguage.Romanian;
            case "ru":
                return SystemLanguage.Russian;
            case "sh":
                return SystemLanguage.SerboCroatian;
            case "sk":
                return SystemLanguage.Slovak;
            case "sl":
                return SystemLanguage.Slovenian;
            case "es":
                return SystemLanguage.Spanish;
            case "sv":
                return SystemLanguage.Swedish;
            case "th":
                return SystemLanguage.Thai;
            case "tr":
                return SystemLanguage.Turkish;
            case "uk":
                return SystemLanguage.Ukrainian;
            case "vi":
                return SystemLanguage.Vietnamese;
            default:
                return SystemLanguage.English; //Default to English if the language is not in the list
        }
    }

    public string ToJSON(string prefix)
    {
        string json = "";

        int index = 0;
        foreach (KeyValuePair<SystemLanguage, string> pair in rows)
        {
            string languageCode = LanguageToCode(pair.Key);
            string fieldName = englishFieldName + "_" + languageCode;
            json += $"\n{prefix}\"{fieldName}\": \"{pair.Value}\"";
            if (index++ < rows.Count - 1)
            {
                json += $",";
            }
            // else
            // {
            //     json += $"\n";
            // }
        }

        if (index == 0)
        {
            json += $"\n{prefix}\"{englishFieldName}\": \"\"";
        }

        return json;
    }

    public override string ToString()
    {
        return rows.ToString();
    }

    public IEnumerable<KeyValuePair<SystemLanguage, string>> GetTranslations()
    {
        return rows;
    }

    public string GetFieldKey(SystemLanguage language)
    {
        return englishFieldName + "_" + LanguageToCode(language);
    }
}