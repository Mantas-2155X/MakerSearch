using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using CharaCustom;
using HarmonyLib;
using UnityEngine;

namespace HS2_MakerSearch
{
    [BepInProcess("HoneySelect2")]
    [BepInPlugin(nameof(HS2_MakerSearch), nameof(HS2_MakerSearch), VERSION)]
    public class HS2_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.5.0";

        public static string searchString;
        public static string TranslationCachePath;
        public static int lastControllerIdx;
        
        public static byte sex;
        
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
            searchTextMemory = Config.Bind(new ConfigDefinition("General", "Search text memory"), Tools.SearchTextMemory.Separate, new ConfigDescription("Global - search text is same for all boxes, \nSeparate - different for each box/category, \nNone - reset after search"));
            searchName = Config.Bind(new ConfigDefinition("Search", "Include name"), true);
            searchAssetBundle = Config.Bind(new ConfigDefinition("Search", "Include assetbundle"), false);
            searchAuthor = Config.Bind(new ConfigDefinition("Search", "Include author"), true);

            var harmony = Harmony.CreateAndPatchAll(typeof(Hooks));
            
            var iteratorType = typeof(CvsBase).GetNestedType("<>c__DisplayClass48_0", AccessTools.all);
            var iteratorMethod = AccessTools.Method(iteratorType, "<Start>b__5");
            var postfix = new HarmonyMethod(typeof(Hooks), nameof(Hooks.Patch_ResetSearch));
            harmony.Patch(iteratorMethod, null, postfix);
            
            TranslationCachePath = Path.Combine(Paths.CachePath, "HS2_MakerSearch.cache");
        }

        public static void Search(int controllerIdx)
        {
            if (searchTextMemory.Value == Tools.SearchTextMemory.Global)
                foreach (var field in Tools.fields.Where(field => field != null))
                    field.text = searchString;
            
            if (!Tools.UpdateUI(controllerIdx))
                return;

            if (searchString == "")
                return;

            lastControllerIdx = controllerIdx;
            
            var controller = Tools.controllers[controllerIdx];
            var datas = controller.scrollerDatas;

            var datalist = datas.ToList();
            foreach (var data in datalist.ToArray())
            {
                if(Tools.ItemMatchesSearch(data.info, searchString))
                    continue;

                if (controller.selectInfo == data)
                    controller.SelectInfoClear();

                datalist.Remove(data);
            }
            datas = datalist.ToArray();

            controller.scrollerDatas = datas;
            controller.view.ReSetListItemCount(Mathf.CeilToInt((float)datas.Length / controller.countPerRow));
        }
    }
}