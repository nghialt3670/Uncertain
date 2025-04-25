using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.Localization;

public static class LocalizationUtils
{
    public static string ConvertCodeToNativeName(string code)
    {
        return new CultureInfo(code).NativeName;
    }

    public static string ConvertNativeNameToCode(string nativeName)
    {
        return CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .FirstOrDefault(info => info.NativeName == nativeName)
            .Name;
    }

    public static Locale ConvertCodeToLocale(string code)
    {
        return UnityEngine
                .Localization
                .Settings
                .LocalizationSettings
                .AvailableLocales
                .Locales
                .FirstOrDefault(locale => locale.Identifier.Code == code);
    }

    public static string ConvertLocaleToCode(Locale locale)
    {
        return locale.Identifier.Code;
    }

    public static string ConvertLocaleToNativeName(Locale locale)
    {
        return ConvertCodeToNativeName(locale.Identifier.Code);
    }

    public static Locale ConvertNativeNameToLocale(string nativeName)
    {
        return ConvertCodeToLocale(ConvertNativeNameToCode(nativeName));
    }

    public static void SetLocale(Locale locale)
    {
        UnityEngine
            .Localization
            .Settings
            .LocalizationSettings
            .SelectedLocale = locale;
    }

    public static void SetLocaleByCode(string code)
    {
        SetLocale(ConvertCodeToLocale(code));
    }

    public static void SetLocaleByNativeName(string nativeName)
    {
        SetLocale(ConvertNativeNameToLocale(nativeName));
    }

    public static List<Locale> GetAvailableLocales()
    {
        return UnityEngine
            .Localization
            .Settings
            .LocalizationSettings
            .AvailableLocales
            .Locales;
    }

    public static List<string> GetAvailableCodes()
    {
        return GetAvailableLocales().Select(ConvertLocaleToCode).ToList();
    }

    public static List<string> GetAvailableNativeNames()
    {
        return GetAvailableLocales().Select(ConvertLocaleToNativeName).ToList();
    }
}
