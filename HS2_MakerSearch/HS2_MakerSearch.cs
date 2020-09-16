using System.IO;
using System.Linq;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using CharaCustom;
using SuperScrollView;

using UnityEngine;

namespace HS2_MakerSearch
{
    [BepInProcess("HoneySelect2")]
    [BepInPlugin(nameof(HS2_MakerSearch), nameof(HS2_MakerSearch), VERSION)]
    public class HS2_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.4.0";

        public static string searchString;
        public static string TranslationCachePath;

        public static CvsH_Hair cvsHair;
        
        public static CvsC_Clothes cvsClothes;
        
        public static CvsA_Slot cvsAccessories;
        
        public static CvsB_Skin cvsSkin;
        public static CvsB_Sunburn cvsSunburn;
        public static CvsB_Nip cvsNip;
        public static CvsB_Underhair cvsUnderhair;
        public static CvsB_Paint cvsPaint;
        
        public static CvsF_Mole cvsMole;
        public static CvsF_EyeLR cvsEye;
        public static CvsF_EyeHL cvsHighlight;
        public static CvsF_Eyebrow cvsEyebrow;
        public static CvsF_Eyelashes cvsEyelash;

        public static CvsF_MakeupEyeshadow cvsEyeshadow;
        public static CvsF_MakeupCheek cvsCheek;
        public static CvsF_MakeupLip cvsLip;
        public static CvsF_MakeupPaint cvsFacePaint;
        
        public static LoopListView2 view;
        public static CustomSelectScrollController controller;
        
        public static Tools.SearchCategory category;

        public static byte sex;
        
        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }
        
        public static ConfigEntry<Tools.SearchBy> searchBy { get; private set; }
        public static ConfigEntry<Tools.SearchTextMemory> searchTextMemory { get; private set; }

        
        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));
            searchBy = Config.Bind(new ConfigDefinition("General", "Search by"), Tools.SearchBy.Name);
            searchTextMemory = Config.Bind(new ConfigDefinition("General", "Search text memory"), Tools.SearchTextMemory.Separate, new ConfigDescription("Global - search text is same for all boxes, \nSeparate - different for each box/category, \nNone - reset after search"));

            category = Tools.SearchCategory.None;
            
            var harmony = new Harmony(nameof(HS2_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
            
            TranslationCachePath = Path.Combine(Paths.CachePath, "HS2_MakerSearch.cache");
        }

        public static void Search()
        {
            if (searchTextMemory.Value == Tools.SearchTextMemory.Global)
                foreach (var field in Tools.fields.Where(field => field != null))
                    field.text = searchString;
            
            if (!Tools.UpdateUI(category))
                return;

            if (searchString == "")
                return;

            var trav = Traverse.Create(controller);
            var datas = trav.Field("scrollerDatas").GetValue<CustomSelectScrollController.ScrollData[]>();

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

            trav.Field("scrollerDatas").SetValue(datas);

            view.ReSetListItemCount(Mathf.CeilToInt((float)datas.Length / trav.Field("countPerRow").GetValue<int>()));
        }
    }
}