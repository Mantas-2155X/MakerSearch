using System.Linq;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using CharaCustom;
using SuperScrollView;

using UnityEngine;

namespace AI_MakerSearch
{
    [BepInProcess("AI-Syoujyo")]
    [BepInProcess("AI-Shoujo")]
    [BepInPlugin(nameof(AI_MakerSearch), nameof(AI_MakerSearch), VERSION)]
    public class AI_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.1.1";

        public static bool isSteam;
        public static string searchString;
        
        public static CvsH_Hair cvsHair;
        public static CvsC_Clothes cvsClothes;
        public static CvsA_Slot cvsAccessories;
        public static CvsB_Skin cvsSkin;
        public static CvsB_Sunburn cvsSunburn;
        public static CvsB_Nip cvsNip;
        public static CvsB_Underhair cvsUnderhair;
        public static CvsB_Paint cvsPaint;
        
        public static LoopListView2 view;
        public static CustomSelectScrollController controller;
        
        public static Tools.SearchCategory category;

        public static byte sex;
        
        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }
        
        public static ConfigEntry<Tools.SearchBy> searchBy { get; private set; }
        
        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));
            searchBy = Config.Bind(new ConfigDefinition("General", "Search by"), Tools.SearchBy.Name);

            category = Tools.SearchCategory.None;
            
            var harmony = new Harmony(nameof(AI_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
            
            isSteam = Application.productName == "AI-Shoujo";
        }

        public static void Search()
        {
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