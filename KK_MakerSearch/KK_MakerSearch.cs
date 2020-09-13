using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

using ChaCustom;
using UnityEngine;

namespace KK_MakerSearch
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInPlugin(nameof(KK_MakerSearch), nameof(KK_MakerSearch), VERSION)]
    public class KK_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.2.1";

        public static string searchString;
        
        public static CustomSelectListCtrl ctrl;
        public static CanvasGroup acsGrp;
        
        public static bool DisableHiddenTabs;
        
        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }
        
        public static ConfigEntry<Tools.SearchBy> searchBy { get; private set; }
        
        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));
            searchBy = Config.Bind(new ConfigDefinition("General", "Search by"), Tools.SearchBy.Name);

            var MakerOptimizations = Type.GetType("IllusionFixes.MakerOptimizations, KK_Fix_MakerOptimizations");
            if (MakerOptimizations != null)
            {
                if (!(MakerOptimizations.GetField("<DisableHiddenTabs>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is ConfigEntry<bool> field))
                    return;
                
                if (field.Value)
                    DisableHiddenTabs = true;
            }
            
            var harmony = new Harmony(nameof(KK_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
        }
        
        public static void Search()
        {
            Tools.ResetDisables();

            if (searchString == "")
                return;

            var trav = Traverse.Create(ctrl);
            var datas = trav.Field("lstSelectInfo").GetValue<List<CustomSelectInfo>>();

            foreach (var t in datas.Where(t => !Tools.ItemMatchesSearch(t, searchString)))
                ctrl.DisvisibleItem(t.index, true);
        }
    }
}