using JLPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ATS_API.Helpers;
using ATS_API.Localization;
using ATS_JSONLoader;
using ATS_JSONLoader.Sounds;
using Eremite;
using Eremite.Model;
using Eremite.Model.Sound;
using TinyJson;
using UnityEngine;
using Plugin = ATS_JSONLoader.Plugin;

public static partial class ImportExportUtils
{
    private static string ID;
    private static string DebugPath;
    private static string LoggingSuffix;

    public static void SetID(string id)
    {
        ID = id;
    }

    public static void SetDebugPath(string path)
    {
        DebugPath = path.Substring(Plugin.BepInExDirectory.Length);
        LoggingSuffix = "";
    }

    public static T ParseEnum<T>(string value) where T : unmanaged, Enum
    {
        T result;
        if (Enum.TryParse<T>(value, out result))
            return result;

        int idx = Math.Max(value.LastIndexOf('_'), value.LastIndexOf('.'));

        if (idx < 0)
            throw new InvalidCastException($"Cannot parse {value} as {typeof(T).FullName}");

        string guid = value.Substring(0, idx);
        string name = value.Substring(idx + 1);
        return GUIDManager.Get<T>(guid, name);
    }
    
    public static void Applymethod<T, Y>(Func<T> getter, Action<T> setter, ref Y serializeInfoValue, bool toModel, string category, string suffix)
    {
        if (toModel)
        {
            T t = default;
            ApplyValue(ref t, ref serializeInfoValue, true, category, suffix);
            setter(t);

        }
        else
        {
            T t = getter();
            ApplyValue(ref t, ref serializeInfoValue, false, category, suffix);
        }
    }

    public static void ApplyProperty<T, Y>(Func<T> modelGetter, Action<T> modelSetter, ref Y serializeInfoValue, bool toModel, string category, string suffix)
    {
        if (toModel)
        {
            T t = default;
            ApplyValue(ref t, ref serializeInfoValue, true, category, suffix);
            modelSetter(t);

        }
        else
        {
            T t = modelGetter();
            ApplyValue(ref t, ref serializeInfoValue, false, category, suffix);
        }
    }

    public static void ApplyProperty<T, Y>(ref T serializeInfoValue, Func<Y> getter, Action<Y> setter, bool toModel, string category, string suffix)
    {
        if (toModel)
        {
            Y y = getter();
            ApplyValue(ref serializeInfoValue, ref y, false, category, suffix);
        }
        else
        {
            Y y = default;
            ApplyValue(ref serializeInfoValue, ref y, true, category, suffix);
            setter(y);
        }
    }

    public static void ApplyValue<T, Y>(ref T a, ref Y b, bool toA, string category, string suffix)
    {
        if (toA)
        {
            ConvertValue(ref b, ref a, category, suffix);
        }
        else
        {
            ConvertValue(ref a, ref b, category, suffix);
        }
    }
    
    public static void ApplyVector2<T>(ref Vector2 a, ref T bX, ref T bY, bool toA, string category, string suffix)
    {
        if (toA)
        {
            ConvertValue(ref bX, ref a.x, category, suffix);
            ConvertValue(ref bY, ref a.y, category, suffix);
        }
        else
        {
            ConvertValue(ref a.x, ref bX, category, suffix);
            ConvertValue(ref a.y, ref bY, category, suffix);
        }
    }

    public static void ApplyValueNoNull<T, Y>(ref T a, ref Y b, bool toA, string category, string suffix)
    {
        if (toA)
        {
            if ((object)b != GetDefault(typeof(Y)))
            {
                ConvertValue(ref b, ref a, category, suffix);
            }
            else
            {
                VerboseLog($"Skipping {category}.{suffix} as it is null");
            }
        }
        else
        {
            if ((object)a != GetDefault(typeof(T)))
            {
                ConvertValue(ref a, ref b, category, suffix);
            }
            else
            {
                VerboseLog($"Skipping {category}.{suffix} as it is null");
            }
        }
    }

    private static void ConvertValue<FromType, ToType>(ref FromType from, ref ToType to, string category, string suffix)
    {
        LoggingSuffix = suffix;

        Type fromType = typeof(FromType);
        Type toType = typeof(ToType);
        try
        {
            if (fromType == toType)
            {
                to = (ToType)(object)from;
                return;
            }
            else if (AreNullableTypesEqual(from, to, out object fromValue, out object _, out bool fromHasValue,
                         out bool _))
            {
                //Debug.Log($"Same types and someone is nullable");
                if (fromHasValue)
                {
                    to = (ToType)fromValue;
                }

                return;
            }
            else if (fromType.IsGenericType && fromType.GetGenericTypeDefinition() == typeof(List<>) &&
                     toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // List to List
                //Plugin.Log.LogInfo($"List to List {from} {to}");
                if (from != null)
                {
                    IList toList = (IList)Activator.CreateInstance(toType);
                    to = (ToType)toList;
                    IList fromList = (IList)from;
                    for (int i = 0; i < fromList.Count; i++)
                    {
                        var o1 = fromList[i];
                        var o2 = GetDefault(toType.GetGenericArguments().Single());
                        var converted = ConvertType(fromType, toType, o1, o2, category, $"{suffix}_{i + 1}");
                        toList.Add(converted);
                    }
                }

                //Plugin.Log.LogInfo($"List to List done with {to}");
                return;
            }
            else if (fromType.IsGenericType && fromType.GetGenericTypeDefinition() == typeof(List<>) && toType.IsArray)
            {
                // List to Array
                //Plugin.Log.LogInfo($"List to Array {from} {to}");
                if (from != null)
                {
                    IList fromList = (IList)from;
                    int size = from == null ? 0 : fromList.Count;
                    Array toArray = Array.CreateInstance(toType.GetElementType(), size);
                    to = (ToType)(object)toArray;
                    for (int i = 0; i < fromList.Count; i++)
                    {
                        var o1 = fromList[i];
                        var o2 = GetDefault(toType.GetElementType());

                        object[] parameters = { o1, o2, category, $"{suffix}_{i + 1}" };
                        var m = typeof(ImportExportUtils).GetMethod(nameof(ConvertValue),
                                BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(fromType.GetGenericArguments().Single(), toType.GetElementType());

                        m.Invoke(null, parameters);

                        toArray.SetValue(parameters[1], i);
                    }
                }

                //Plugin.Log.LogInfo($"List to Array Done {from} {to}");
                return;
            }
            else if (fromType.IsArray && toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Array to List
                if (from != null)
                {
                    IList toList = (IList)Activator.CreateInstance(toType);
                    to = (ToType)toList;
                    Array fromArray = (Array)(object)from;
                    for (int i = 0; i < fromArray.Length; i++)
                    {
                        var o1 = fromArray.GetValue(i);
                        var o2 = GetDefault(toType.GetGenericArguments().Single());

                        object[] parameters = { o1, o2, category, $"{suffix}_{i + 1}" };
                        var m = typeof(ImportExportUtils).GetMethod(nameof(ConvertValue),
                                BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(fromType.GetElementType(), toType.GetGenericArguments().Single());

                        m.Invoke(null, parameters);

                        toList.Add(parameters[1]);
                    }
                }

                return;
            }

            else if (fromType.IsArray && toType.IsArray)
            {
                // Array to Array
                if (from != null)
                {
                    Array fromList = (Array)(object)from;
                    int size = from == null ? 0 : fromList.Length;
                    Array toArray = Array.CreateInstance(toType.GetElementType(), size);
                    to = (ToType)(object)toArray;
                    for (int i = 0; i < fromList.Length; i++)
                    {
                        var o1 = fromList.GetValue(i);
                        var o2 = GetDefault(toType.GetElementType());

                        object[] parameters = { o1, o2, category, $"{suffix}_{i + 1}" };
                        var m = typeof(ImportExportUtils).GetMethod(nameof(ConvertValue),
                                BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(fromType.GetElementType(), toType.GetElementType());

                        m.Invoke(null, parameters);

                        toArray.SetValue(parameters[1], i);
                    }
                }

                return;
            }
            else if (fromType.IsEnum && toType == typeof(string))
            {
                string oType = from.ToString();
                if (int.TryParse(oType, out int value))
                {
                    if (Enum.GetValues(fromType).Cast<int>().Contains(value))
                    {
                        to = (ToType)(object)oType;
                        return;
                    }

                    // NOTE: This is for exporting which we do not support atm
                    // // Custom type
                    // object[] parameters = { value, "guid", "name" };
                    // var m = typeof(GUIDManager).GetMethod(nameof(GUIDManager.TryGetGuidAndKeyEnumValue), BindingFlags.Public | BindingFlags.Static)
                    //     .MakeGenericMethod(fromType);
                    // var result = (bool)m.Invoke(null, parameters);
                    //
                    // if (result)
                    // {
                    //     string guid = (string)parameters[1];
                    //     string key = (string)parameters[2];
                    //     to = (ToType)(object)(guid + "_" + key);
                    // }
                    // else
                    {
                        Error($"Failed to convert enum to string! '{from}' int '{value}'");
                        to = (ToType)(object)oType;
                    }
                }
                else
                {
                    to = (ToType)(object)oType;
                }

                return;
            }
            else if (fromType == typeof(string) && toType.IsEnum)
            {
                if (!string.IsNullOrEmpty((string)(object)from))
                {
                    object o = typeof(ImportExportUtils)
                        .GetMethod(nameof(ParseEnum), BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(toType)
                        .Invoke(null, new object[] { from });
                    to = (ToType)o;
                }

                return;
            }
            // else if (fromType == typeof(SO) && toType == typeof(string))
            // {
            //     if (from != null)
            //         to = (ToType)(object)((from as SO).name);
            //     return;
            // }
            // else if (fromType == typeof(string) && toType == typeof(CardInfo))
            // {
            //     string s = (string)(object)from;
            //     if (s != null)
            //     {
            //         s = s.Trim(); // Avoid people adding spaces in for blank cards.....
            //     }
            //
            //     if (!string.IsNullOrEmpty(s))
            //     {
            //         to = (ToType)(object)CardManager.AllCardsCopy.CardByName(s);
            //         if (to == null)
            //         {
            //             // Name in the JSONLoader is incorrect. See if we can find it and tell the user
            //             CardInfo[] infos = FindSimilarCards(s);
            //             if (infos.Length == 0)
            //             {
            //                 string allCardInfos = string.Join(", ", CardManager.AllCardsCopy.Select(c => c.name));
            //                 Error($"Could not find CardInfo with name '{s}'!\nAllCards: {allCardInfos}");
            //             }
            //             else
            //             {
            //                 to = (ToType)(object)infos[0];
            //                 string cardNames = string.Join(" or ", infos.Select((a) => "'" + a.name + "'"));
            //                 Warning($"Could not find CardInfo with name '{s}'. Did you mean {cardNames}?");
            //             }
            //         }
            //     }
            //
            //     return;
            // }
            else if (fromType == typeof(string) && (toType == typeof(Texture) || toType.IsSubclassOf(typeof(Texture))))
            {
                string path = (string)(object)from;
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        to = (ToType)(object)GetTextureFromString(path);
                    }
                    catch (FileNotFoundException)
                    {
                        Error($"Failed to find texture {path}!");
                    }
                }

                return;
            }
            else if ((fromType == typeof(Texture) || fromType.IsSubclassOf(typeof(Texture))) &&
                     toType == typeof(string))
            {
                Texture texture = (Texture)(object)from;
                if (texture != null)
                {
                    string path = Path.Combine(Plugin.ExportDirectory, category, "Assets", $"{ID}_{suffix}.png");
                    to = (ToType)(object)ExportTexture(texture, path);
                }

                return;
            }
            else if (fromType == typeof(string) && toType == typeof(Sprite))
            {
                string path = (string)(object)from;
                if (!string.IsNullOrEmpty(path))
                {
                    Texture2D imageAsTexture = GetTextureFromString(path);
                    if (imageAsTexture != null)
                    {
                        to = (ToType)(object)imageAsTexture.ConvertTexture();
                    }
                }

                return;
            }
            else if (fromType == typeof(Sprite) && toType == typeof(string))
            {
                Sprite texture = (Sprite)(object)from;
                if (texture != null)
                {
                    string path = Path.Combine(Plugin.ExportDirectory, category, "Assets", $"{ID}_{suffix}.png");
                    to = (ToType)(object)ExportTexture(texture.texture, path);
                }

                return;
            }
            else if (fromType.GetInterfaces().Contains(typeof(IConvertible)) &&
                     toType.GetInterfaces().Contains(typeof(IConvertible)))
            {
                IConvertible a = from as IConvertible;
                IConvertible b = to as IConvertible;
                if (a != null && b != null)
                {
                    to = (ToType)Convert.ChangeType(a, toType);
                }

                return;
            }
            else if (fromType == typeof(string) && toType == typeof(Color))
            {
                string value = (string)(object)from;
                Color color = Color.white;
                if (value.StartsWith("#"))
                {
                    if (!ColorUtility.TryParseHtmlString(value, out color))
                    {
                        Error($"Could not convert {value} to color!");
                    }
                }
                else
                {
                    int[] split = value.Split(',').Select((a) => int.Parse(a.Trim())).ToArray();
                    if (split.Length > 0)
                    {
                        color.r = split[0] / 255f;
                    }

                    if (split.Length > 1)
                    {
                        color.g = split[1] / 255f;
                    }

                    if (split.Length > 2)
                    {
                        color.b = split[2] / 255f;
                    }

                    if (split.Length > 3)
                    {
                        color.a = split[3] / 255f;
                    }
                }

                to = (ToType)(object)color;
                return;
            }
            else if (fromType == typeof(Color) && toType == typeof(string))
            {
                Color color = (Color)(object)from;
                to = (ToType)(object)$"{color.r * 255:F0},{color.g * 255:F0},{color.b * 255:F0},{color.a * 255:F0}";
                return;
            }
            else if (fromType == typeof(string) && toType == typeof(Vector2))
            {
                string value = (string)(object)from;
                string[] split = value.Split(',');
                if (split.Length == 2)
                {
                    to = (ToType)(object)new Vector2(float.Parse(split[0]), float.Parse(split[1]));
                }
                else
                {
                    Error($"Could not convert {value} to Vector2!");
                }

                return;
            }
            else if (fromType == typeof(Vector2) && toType == typeof(string))
            {
                Vector2 vector = (Vector2)(object)from;
                to = (ToType)(object)$"{vector.x},{vector.y}";
                return;
            }
            else if (fromType.IsSubclassOf(typeof(ScriptableObject)) && toType == typeof(string))
            {
                ScriptableObject so = (ScriptableObject)(object)from;
                to = (ToType)(object)so.name;
                return;
            }
            else if (fromType == typeof(AudioClip) && toType == typeof(string))
            {
                AudioClip clip = (AudioClip)(object)from;
                if (clip != null)
                {
                    string path = Path.Combine(Plugin.ExportDirectory, category, "Audio", $"{ID}_{suffix}.wav");
                    to = (ToType)(object)AudioHelpers.ExportAudioClip(clip, path);
                }
                to = (ToType)(object)clip.name;
                return;
            }
            else if (fromType == typeof(string) && toType == typeof(AudioClip))
            {
                string path = (string)(object)from;
                AudioClip audioClip = AudioHelpers.LoadAudioClip(path);
                to = (ToType)(object)audioClip;
                return;
            }
            else if (fromType == typeof(string) && toType == typeof(NeedModel))
            {
                string path = (string)(object)from;
                NeedModel need = path.ToNeedModel();
                to = (ToType)(object)need;
                return;
            }
            else if (fromType == typeof(string) && toType == typeof(ModelTag))
            {
                string path = (string)(object)from;
                ModelTag tag = path.ToModelTag();
                to = (ToType)(object)tag;
                return;
            }
            else if (fromType == typeof(LocalizableField) && toType == typeof(string))
            {
                Error("Use ApplyLocaleField when converted from LocalizableField to string!");
            }
            else if (fromType == typeof(string) && toType == typeof(LocalizableField))
            {
                Error("Use ApplyLocaleField when converted from string to LocalizableField!");
            }
            else if (fromType == typeof(RacialSound) && toType == typeof(RacialSounds))
            {
                RacialSound fromSounds = (RacialSound)(object)from;
                RacialSounds toSounds = new RacialSounds();
                ApplyValue(ref fromSounds.positiveSound, ref toSounds.PositiveSounds, false, category, suffix+"_PositiveSounds");
                ApplyValue(ref fromSounds.neutralSound, ref toSounds.NeutralSounds, false, category, suffix+"_NeutralSounds");
                ApplyValue(ref fromSounds.negativeSound, ref toSounds.NegativeSounds, false, category, suffix+"_NegativeSounds");
                to = (ToType)(object)toSounds;
                return;
            }
            else if (fromType == typeof(RacialSounds) && toType == typeof(RacialSound))
            {
                RacialSounds fromSounds = (RacialSounds)(object)from;
                RacialSound toSounds = ScriptableObject.CreateInstance<RacialSound>();
                ApplyValue(ref fromSounds.PositiveSounds, ref toSounds.positiveSound, false, category, suffix+"_PositiveSounds");
                ApplyValue(ref fromSounds.NeutralSounds, ref toSounds.neutralSound, false, category, suffix+"_NeutralSounds");
                ApplyValue(ref fromSounds.NegativeSounds, ref toSounds.negativeSound, false, category, suffix+"_NegativeSounds");
                to = (ToType)(object)toSounds;
                return;
            }
            else if (fromType == typeof(SoundRef) && toType == typeof(SoundCollection))
            {
                SoundRef soundRef = (SoundRef)(object)from;
                SoundCollection soundCollection = new SoundCollection();
                soundCollection.Initialize();
                if (soundRef != null && soundRef.sounds != null)
                {
                    foreach (SoundModel model in soundRef.sounds)
                    {
                        if (model == null || model.audioClip == null)
                        {
                            continue;
                        }
                        
                        Sound sound = new Sound();
                        ApplyValue(ref sound.soundPath, ref model.audioClip, true, category, suffix + "_audioClip");
                        ApplyValue(ref sound.volume, ref model.volume, true, category, suffix + "_volume");
                        soundCollection.sounds.Add(sound);
                    }
                }

                to = (ToType)(object)soundCollection;
                return;
            }
            else if (fromType == typeof(SoundCollection) && toType == typeof(SoundRef))
            {
                SoundCollection soundCollection = (SoundCollection)(object)from;
                SoundRef soundRef = ScriptableObject.CreateInstance<SoundRef>();
                if (soundRef != null)
                {
                    soundRef.sounds = new SoundModel[soundCollection.sounds.Count];
                    for (int i = 0; i < soundCollection.sounds.Count; i++)
                    {
                        SoundModel model = new SoundModel();
                        Sound sound = soundCollection.sounds[i];
                        ApplyValue(ref sound.soundPath, ref model.audioClip, false, suffix, suffix + "_audioClip");
                        ApplyValue(ref sound.volume, ref model.volume, false, suffix, suffix + "_volume");
                        soundRef.sounds[i] = model;
                    }
                }

                to = (ToType)(object)soundRef;
                return;
            }
        }
        catch (Exception e)
        {
            Error($"Failed to convert: {fromType} to {toType}");
            Exception(e);
            return;
        }

        Error($"Unsupported conversion type: {fromType} to {toType}\n{Environment.StackTrace}");
    }

    private static Texture2D GetTextureFromString(string path)
    {
        if (path.StartsWith("base64:"))
        {
            try
            {
                string contents = path.Substring("base64:".Length);
                byte[] bytes = Convert.FromBase64String(contents);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.LoadImage(bytes);
                
                return texture;
            }
            catch (Exception e)
            {
                Error($"Failed to convert base64 to texture: {path}");
                throw e;
            }
        }

        return TextureHelper.GetImageAsTexture(path);
    }

    /// <summary>
    /// Finds similar cards with the same name
    /// </summary>
    /// <returns></returns>
    public static string[] FindSimilarStrings(string misspelledCardName, IEnumerable<string> collection)
    {
        return FindSimilar(misspelledCardName, collection, static (a) => a);
    }
    
    
    /// <summary>
    /// Find elements that are similar to the misspelled string by
    /// Comparing each character looking to see if they match
    /// Ignores case sensitivity, - and _
    /// </summary>
    public static T[] FindSimilar<T>(string misspelledCardName, IEnumerable<T> allElements, Func<T,string> getter)
    {
        const int maxErrors = 4; // Minimum mistakes before we include the option
        List<Tuple<int, T>> cardInfos = new List<Tuple<int, T>>();

        string sourceCardName = misspelledCardName.ToLower().Replace("-", "").Replace("_", "");
        int errorMargin = Mathf.Clamp(sourceCardName.Length - 1, 1, maxErrors);
        foreach (T cardInfo in allElements)
        {
            string realCardName = getter(cardInfo).ToLower().Replace("-", "").Replace("_", "");

            // Skip cards that are TOOO different to ours
            if (Mathf.Abs(sourceCardName.Length - realCardName.Length) > errorMargin)
                continue;

            int match = 0;
            int errors = Mathf.Max(0, sourceCardName.Length - realCardName.Length);

            // Go from right to left because most cards have a GUID at the start
            int j = realCardName.Length - 1;
            for (int i = sourceCardName.Length - 1; i >= 0 && j >= 0; --i, --j)
            {
                if (realCardName[j] == sourceCardName[i])
                {
                    match++;
                }
                else
                {
                    errors++;

                    // if the margin of error is too small, skip
                    if (errors > errorMargin)
                        break;

                    if (j > 0 && realCardName[j - 1] == sourceCardName[i])
                    {
                        // Maybe didn't add a character
                        // realCardName = LFTD_Zombie
                        // sourceCardName = LFTDZombie
                        j--;
                        match++;
                    }
                    else if (i > 0 && realCardName[j] == sourceCardName[i - 1])
                    {
                        // We have an extra character
                        // realCardName = LFTD_Scavenger
                        // sourceCardName = LFTD_Scavenger1
                        i--;
                        match++;
                    }
                }
            }

            if (match > 0 && errors < errorMargin)
                cardInfos.Add(new Tuple<int, T>(match, cardInfo));
        }

        // Sort by highest match
        cardInfos.Sort((a, b) => b.Item1 - a.Item1);
        return cardInfos.Select((a) => a.Item2).ToArray();
    }

    private static object ConvertType(Type fromType, Type toType, object o1, object o2, string category, string suffix)
    {
        object[] parameters = { o1, o2, category, suffix };
        var m = typeof(ImportExportUtils).GetMethod(nameof(ConvertValue), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(fromType.GetGenericArguments().Single(), toType.GetGenericArguments().Single());

        m.Invoke(null, parameters);
        return parameters[1];
    }

    private static bool AreNullableTypesEqual<T, Y>(T t, Y y, out object a, out object b, out bool aHasValue, out bool bHasValue)
    {
        //Debug.Log($"AreNullableTypesEqual: {typeof(T)} to {typeof(Y)}");
        aHasValue = false;
        bHasValue = false;
        a = null;
        b = null;

        bool tIsNullable = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        bool yIsNullable = typeof(Y).IsGenericType && typeof(Y).GetGenericTypeDefinition() == typeof(Nullable<>);
        if (!tIsNullable && !yIsNullable)
        {
            //Debug.Log($"\t Neither are nullable");
            return false;
        }

        Type tInnerType = tIsNullable ? Nullable.GetUnderlyingType(typeof(T)) : typeof(T);
        Type yInnerType = yIsNullable ? Nullable.GetUnderlyingType(typeof(Y)) : typeof(Y);
        if (tInnerType == yInnerType)
        {
            //Debug.Log($"\t Same Inner types: {t}({tInnerType}) {y}({yInnerType})");
            if (tIsNullable)
            {
                a = GetValueFromNullable(t, out aHasValue);
            }
            else
            {
                a = t;
                aHasValue = true;
            }

            if (yIsNullable)
            {
                b = GetValueFromNullable(y, out bHasValue);
            }
            else
            {
                b = y;
                bHasValue = true;
            }

            return true;
        }

        Error($"Not same types {typeof(T)} {typeof(Y)}");
        return false;
    }

    private static string ExportTexture(Texture texture, string path)
    {
        if (texture is Texture2D texture2D)
        {
            return ExportTexture(texture2D, path);
        }

        Texture2D converted = Texture2D.CreateExternalTexture(
            texture.width,
            texture.height,
            TextureFormat.RGBA32,
            false, false,
            texture.GetNativeTexturePtr());
        return ExportTexture(converted, path);
    }

    private static string ExportTexture(Texture2D texture, string path)
    {
        if (!texture.isReadable)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(texture.width, texture.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            texture = readableText;
        }

        byte[] bytes = texture.EncodeToPNG();
        if (bytes == null)
        {
            Error("Failed to turn into bytes??");
        }

        if (string.IsNullOrEmpty(path))
        {
            Error("path is empty????");
        }

        var dirPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        File.WriteAllBytes(path, bytes);
        return Path.GetFileName(path);
    }

    public static string[] ExportTextures(IEnumerable<Texture2D> texture, string type, string fileName)
    {
        int i = 0;
        List<string> paths = new List<string>();
        foreach (Texture2D texture2D in texture)
        {
            i++;

            string path = Path.Combine(Plugin.ExportDirectory, type, "Assets", $"{fileName}_{i}.png");
            paths.Add(ExportTexture(texture2D, path));
        }

        return paths.ToArray();
    }
    
    public static void ApplyLocaleField(string field, ref LocaText model, ref LocalizableField data, bool toModel)
    {
        if (toModel)
        {
            // modelField needs a new key generated from the rows
            string key = "";
            ApplyLocaleField(field, data, ref key);
            model = key.ToLocaText();
        }
        else
        {
            // TODO: Exporting
        }
    }
    
    public static void ApplyLocaText(ref LocaText model, ref LocalizableField data, Action<string, SystemLanguage> setModel, bool toModel, string fieldName)
    {
        
        if (toModel)
        {
            // If we are overriding and translations are empty then we don't change anything
            foreach (KeyValuePair<SystemLanguage,string> pair in data.GetTranslations())
            {
                setModel(pair.Value, pair.Key);
            }
            
            // setModel should be setting model in the builder function so no need to do anything with it here
        }
        else
        {
            data = new LocalizableField(fieldName);

            string languageCode = MB.TextsService.CurrentLocaCode;
            string value = model.GetText();
            string escaped = value.Replace("\n", "\\n");
            data.SetValueWithLanguageCode(languageCode, escaped);
        }
    }

    /// <summary>
    /// From GoodData to GoodModel
    /// </summary>
    /// <param name="field"></param>
    /// <param name="rows"></param>
    /// <param name="modelKey"></param>
    private static void ApplyLocaleField(string field, LocalizableField rows, ref string modelKey)
    {
        modelKey = $"{ID}_{field}";

        VerboseLog($"ApplyLocaleField {field} english {modelKey}");
        foreach (KeyValuePair<SystemLanguage, string> pair in rows.rows)
        {
            SystemLanguage language = pair.Key;
            LocalizationManager.AddString(modelKey, pair.Value, language);
            VerboseLog($"Translation {modelKey} to {language} = {pair.Value}");
        }
    }
    
    private static object GetValueFromNullable<U>(U u, out bool hasValue)
    {
        Type type = typeof(U);
        if (u != null)
        {
            bool v = (bool)type.GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public).GetValue(u);
            if (v)
            {
                hasValue = true;
                return type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetValue(u);
            }
        }

        hasValue = false;
        Type underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType.IsValueType)
        {
            return Activator.CreateInstance(underlyingType);
        }
        return null;
    }

    private static object GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    private static void VerboseLog(string message)
    {
        Logging.VerboseLog($"[{DebugPath}][{ID}][{LoggingSuffix}] {message}");
    }

    private static void VerboseWarning(string message)
    {
        if (Configs.VerboseLogging)
            Logging.VerboseWarning($"[{DebugPath}][{ID}][{LoggingSuffix}] {message}");
    }

    private static void VerboseError(string message)
    {
        if (Configs.VerboseLogging)
            Logging.VerboseError($"[{DebugPath}][{ID}][{LoggingSuffix}] {message}");
    }
    
    private static void Log(string message)
    {
        Plugin.Log.LogInfo($"[{ID}][{LoggingSuffix}] {message}");
    }

    private static void Warning(string message)
    {
        if (Configs.VerboseLogging)
            VerboseWarning(message);
        else
            Plugin.Log.LogWarning($"[{ID}][{LoggingSuffix}] {message}");
    }

    private static void Error(string message)
    {
        if (Configs.VerboseLogging)
            VerboseError(message);
        else
            Plugin.Log.LogError($"[{ID}][{LoggingSuffix}] {message}");
    }

    private static void Exception(Exception e)
    {
        Plugin.Log.LogError($"[{DebugPath}][{ID}][{LoggingSuffix}] {e.Message}\n{e.StackTrace}");
    }
}