using System;
using System.Linq;

using XUnity.AutoTranslator.Plugin.Core;

namespace PH_MakerSearch
{
    public static class Tools
    {
        public static void CreateUI()
        {
            
        }
        
        public static bool ItemMatchesSearch(ThumbnailSelectCell data, string searchStr)
        {
            var searchIn = data.name;

            if (PH_MakerSearch.useTranslatedCache.Value)
                AutoTranslator.Default.TranslateAsync(data.name, result => { searchIn = result.Succeeded ? result.TranslatedText : data.name; });

            var rule = StringComparison.Ordinal;
            if (!PH_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }

            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
    }
}