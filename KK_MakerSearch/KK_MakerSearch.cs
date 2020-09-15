﻿using System.IO;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

using ChaCustom;

namespace KK_MakerSearch
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInPlugin(nameof(KK_MakerSearch), nameof(KK_MakerSearch), VERSION)]
    public class KK_MakerSearch : BaseUnityPlugin
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

            var harmony = new Harmony(nameof(KK_MakerSearch));
            harmony.PatchAll(typeof(Hooks));

            TranslationCachePath = Path.Combine(Paths.CachePath, "KK_MakerSearch.cache");
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