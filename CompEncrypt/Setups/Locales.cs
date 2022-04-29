using System.Globalization;

using CompEncrypt.Interfaces;
using CompEncrypt.Setups;

#if IOS
using Foundation;
#endif

namespace CompEncrypt.Setups
{
    public class Locales 
    {
#if IOS
        public void SetLocale()
        {
            var ci = new CultureInfo(GetCurrent());
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        public string GetCurrent()
        {
            var iosLocaleAuto = NSLocale.AutoUpdatingCurrentLocale.LocaleIdentifier;    // en_FR
            var iosLanguageAuto = NSLocale.AutoUpdatingCurrentLocale.LanguageCode;      // en
            var netLocale = iosLocaleAuto.Replace("_", "-");
            var netLanguage = iosLanguageAuto.Replace("_", "-");

            const string defaultCulture = "en";

            if (NSLocale.PreferredLanguages.Length > 0)
            {
                var pref = NSLocale.PreferredLanguages[0];
                netLanguage = pref.Replace("_", "-");
                try
                {
                    var ci = CultureInfo.CreateSpecificCulture(netLanguage);
                    netLanguage = ci.Name;
                }
                catch
                {
                    netLanguage = defaultCulture;
                }
            }
            else
            {
                netLanguage = defaultCulture;
            }

#if DEBUG
            Console.WriteLine(netLanguage);
#endif

            return netLanguage;
        }
#elif ANDROID
        public void SetLocale()
        {

            var androidLocale = Java.Util.Locale.Default;
            var netLocale = androidLocale.ToString().Replace("_", "-");
            var ci = new CultureInfo(netLocale);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        public string GetCurrent()
        {
            var androidLocale = Java.Util.Locale.Default;
            var netLocale = androidLocale.ToString().Replace("_", "-");

            var ci = new CultureInfo(netLocale);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            return netLocale;
        }
#endif
    }
}
