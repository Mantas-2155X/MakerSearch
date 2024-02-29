using System.IO;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

using ChaCustom;

namespace KKS_MakerSearch
{
    [BepInProcess("KoikatsuSunshine")]
    [BepInProcess("KoikatsuSunshineTrial")]
    [BepInPlugin(nameof(KKS_MakerSearch), nameof(KKS_MakerSearch), VERSION)]
    public class KKS_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.5.2";

        public static string searchString;
        public static string TranslationCachePath;
        
        public static CustomSelectListCtrl ctrl;

        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }
        public static ConfigEntry<bool> searchName { get; private set; }
        public static ConfigEntry<bool> searchAssetBundle { get; private set; }
        public static ConfigEntry<bool> searchAuthor { get; private set; }
        public static ConfigEntry<Tools.SearchTextMemory> searchTextMemory { get; private set; }

        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));
            searchTextMemory = Config.Bind(new ConfigDefinition("General", "Search text memory"), Tools.SearchTextMemory.Separate, new ConfigDescription("Global - search text is same for all boxes, \nSeparate - different for each box, \nNone - reset after search"));
            searchName = Config.Bind(new ConfigDefinition("Search", "Include name"), true);
            searchAssetBundle = Config.Bind(new ConfigDefinition("Search", "Include assetbundle"), false);
            searchAuthor = Config.Bind(new ConfigDefinition("Search", "Include author"), true);

            var harmony = new Harmony(nameof(KKS_MakerSearch));
            harmony.PatchAll(typeof(Hooks));

            TranslationCachePath = Path.Combine(Paths.CachePath, "KKS_MakerSearch.cache");
        }
        
        public static void MakerSearch_Search()
        {
            if (searchTextMemory.Value == Tools.SearchTextMemory.Global)
                foreach (var field in Tools.fields.Where(field => field != null))
                    field.text = searchString;
            
            Tools.MakerSearch_ResetDisables();

            if (searchString == "")
                return;

            var trav = Traverse.Create(ctrl);
            var datas = trav.Field("lstSelectInfo").GetValue<List<CustomSelectInfo>>();

            foreach (var t in datas.Where(t => !Tools.ItemMatchesSearch(t, searchString)))
                ctrl.DisvisibleItem(t.index, true);
        }
    }
}