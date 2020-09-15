﻿using System.IO;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

using ChaCustom;

namespace EC_MakerSearch
{
    [BepInProcess("EmotionCreators")]
    [BepInPlugin(nameof(EC_MakerSearch), nameof(EC_MakerSearch), VERSION)]
    public class EC_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.3.1";

        public static string searchString;
        public static string TranslationCachePath;
        
        public static CustomSelectListCtrl ctrl;

        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }
        public static ConfigEntry<Tools.SearchBy> searchBy { get; private set; }
        
        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));
            searchBy = Config.Bind(new ConfigDefinition("General", "Search by"), Tools.SearchBy.Name);

            var harmony = new Harmony(nameof(EC_MakerSearch));
            harmony.PatchAll(typeof(Hooks));

            TranslationCachePath = Path.Combine(Paths.CachePath, "EC_MakerSearch.cache");
        }
        
        public static void MakerSearch_Search()
        {
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