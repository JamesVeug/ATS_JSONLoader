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
        
        SystemLanguage language = GetLanguageFromKey(languageCode);
        rows[language] = value;
    }
    
    public void SetValue(SystemLanguage language, string value)
    {
        rows[language] = value;
    }
    
    public void SetValueWithLanguageCode(string languageCode, string value)
    {
        SystemLanguage language = GetLanguageFromKey(languageCode);
        rows[language] = value;
    }

    private SystemLanguage GetLanguageFromKey(string languageCode)
    {
        switch (languageCode)
        {
            case "en":
                return SystemLanguage.English;
            case "ru":
                return SystemLanguage.Russian;
            case "zh":
                return SystemLanguage.Chinese;
            case "es":
                return SystemLanguage.Spanish;
            case "fr":
                return SystemLanguage.French;
            case "de":
                return SystemLanguage.German;
            case "it":
                return SystemLanguage.Italian;
            case "pt":
                return SystemLanguage.Portuguese;
            case "ja":
                return SystemLanguage.Japanese;
            case "ko":
                return SystemLanguage.Korean;
            case "ar":
                return SystemLanguage.Arabic;
            case "nl":
                return SystemLanguage.Dutch;
            case "fi":
                return SystemLanguage.Finnish;
            case "sv":
                return SystemLanguage.Swedish;
            case "no":
                return SystemLanguage.Norwegian;
            case "pl":
                return SystemLanguage.Polish;
            case "da":
                return SystemLanguage.Danish;
            case "el":
                return SystemLanguage.Greek;
            case "tr":
                return SystemLanguage.Turkish;
            case "hu":
                return SystemLanguage.Hungarian;
            case "cs":
                return SystemLanguage.Czech;
            case "ro":
                return SystemLanguage.Romanian;
            case "th":
                return SystemLanguage.Thai;
            case "vi":
                return SystemLanguage.Vietnamese;
            case "uk":
                return SystemLanguage.Ukrainian;
            case "id":
                return SystemLanguage.Indonesian;
            case "he":
                return SystemLanguage.Hebrew;
            case "af":
                return SystemLanguage.Afrikaans;
            case "eu":
                return SystemLanguage.Basque;
            case "be":
                return SystemLanguage.Belarusian;
            case "bg":
                return SystemLanguage.Bulgarian;
            case "ca":
                return SystemLanguage.Catalan;
            case "fo":
                return SystemLanguage.Faroese;
            case "is":
                return SystemLanguage.Icelandic;
            case "lv":
                return SystemLanguage.Latvian;
            case "lt":
                return SystemLanguage.Lithuanian;
            case "sh":
                return SystemLanguage.SerboCroatian;
            case "sk":
                return SystemLanguage.Slovak;
            case "sl":
                return SystemLanguage.Slovenian;
            default:
                throw new NotImplementedException($"Language code {languageCode} is not supported");
        }
    }
    
    private string GetKeyFromLanguage(SystemLanguage language)
    {
        switch (language)
        {
        case SystemLanguage.English:
            return "en";
        case SystemLanguage.Russian:
            return "ru";
        case SystemLanguage.ChineseSimplified:
        case SystemLanguage.ChineseTraditional:
        case SystemLanguage.Chinese:
            return "zh";
        case SystemLanguage.Spanish:
            return "es";
        case SystemLanguage.French:
            return "fr";
        case SystemLanguage.German:
            return "de";
        case SystemLanguage.Italian:
            return "it";
        case SystemLanguage.Portuguese:
            return "pt";
        case SystemLanguage.Japanese:
            return "ja";
        case SystemLanguage.Korean:
            return "ko";
        case SystemLanguage.Arabic:
            return "ar";
        case SystemLanguage.Dutch:
            return "nl";
        case SystemLanguage.Finnish:
            return "fi";
        case SystemLanguage.Swedish:
            return "sv";
        case SystemLanguage.Norwegian:
            return "no";
        case SystemLanguage.Polish:
            return "pl";
        case SystemLanguage.Danish:
            return "da";
        case SystemLanguage.Greek:
            return "el";
        case SystemLanguage.Turkish:
            return "tr";
        case SystemLanguage.Hungarian:
            return "hu";
        case SystemLanguage.Czech:
            return "cs";
        case SystemLanguage.Romanian:
            return "ro";
        case SystemLanguage.Thai:
            return "th";
        case SystemLanguage.Vietnamese:
            return "vi";
        case SystemLanguage.Ukrainian:
            return "uk";
        case SystemLanguage.Indonesian:
            return "id";
        case SystemLanguage.Hebrew:
            return "he";
        case SystemLanguage.Afrikaans:
            return "af";
        case SystemLanguage.Basque:
            return "eu";
        case SystemLanguage.Belarusian:
            return "be";
        case SystemLanguage.Bulgarian:
            return "bg";
        case SystemLanguage.Catalan:
            return "ca";
        case SystemLanguage.Faroese:
            return "fo";
        case SystemLanguage.Icelandic:
            return "is";
        case SystemLanguage.Latvian:
            return "lv";
        case SystemLanguage.Lithuanian:
            return "lt";
        case SystemLanguage.SerboCroatian:
            return "sh";
        case SystemLanguage.Slovak:
            return "sk";
        case SystemLanguage.Slovenian:
            return "sl";
            default:
                throw new NotImplementedException($"Language {language} is not supported");
        }
    }

    public string ToJSON(string prefix)
    {
        string json = "";

        int index = 0;
        foreach (KeyValuePair<SystemLanguage, string> pair in rows)
        {
            string languageCode = GetKeyFromLanguage(pair.Key);
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
        return englishFieldName + "_" + GetKeyFromLanguage(language);
    }
}