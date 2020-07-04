using System.Linq;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;
using XUnity.AutoTranslator.Plugin.Core;

using CharaCustom;
using SuperScrollView;

namespace HS2_MakerSearch
{
    [BepInPlugin(nameof(HS2_MakerSearch), nameof(HS2_MakerSearch), VERSION)]
    public class HS2_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";
        
        public static CvsH_Hair cvsHair;
        public static CvsC_Clothes cvsClothes;
        
        public static LoopListView2 view;
        public static CustomSelectScrollController controller;
        
        public static Tools.SearchCategory category;
        
        private static ConfigEntry<bool> caseSensitive { get; set; }
        private static ConfigEntry<bool> useTranslatedCache { get; set; }
        
        private static ConfigEntry<Tools.SearchBy> searchBy { get; set; }
        
        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works on search by Name"));
            searchBy = Config.Bind(new ConfigDefinition("General", "Search by"), Tools.SearchBy.Name);

            category = Tools.SearchCategory.None;
            
            var harmony = new Harmony(nameof(HS2_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
        }

        public static void Search(string text)
        {
            if (!Tools.UpdateUI(category))
                return;

            if (text == "")
                return;
            
            if (caseSensitive.Value)
                text = text.ToLower();

            var trav = Traverse.Create(controller);
            var datas = trav.Field("scrollerDatas").GetValue<CustomSelectScrollController.ScrollData[]>();
            
            var datalist = datas.ToList();
            foreach (var data in datalist.ToArray())
            {
                var str = "";

                switch (searchBy.Value)
                {
                    case Tools.SearchBy.Name:
                        str = data.info.name;
                        
                        if (useTranslatedCache.Value)
                            AutoTranslator.Default.TranslateAsync(data.info.name, result => { str = result.Succeeded ? result.TranslatedText : data.info.name; });
                        break;
                    case Tools.SearchBy.AssetName:
                        str = data.info.assetName;
                        break;
                    case Tools.SearchBy.ID:
                        str = data.info.id.ToString();
                        break;
                }

                if (caseSensitive.Value)
                    str = str.ToLower();

                if (str.Contains(text)) 
                    continue;
                
                if (controller.selectInfo == data)
                    controller.SelectInfoClear();
                            
                datalist.Remove(data);
            }
            datas = datalist.ToArray();
            
            trav.Field("scrollerDatas").SetValue(datas);
            
            var num = datas.Length / trav.Field("countPerRow").GetValue<int>();
            view.ReSetListItemCount(num);
        }
    }
}