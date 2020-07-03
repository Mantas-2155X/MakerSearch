using System.Linq;

using BepInEx;
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
        
        private static CvsH_Hair cvsHair;
        private static CvsC_Clothes cvsClothes;
        
        private static LoopListView2 view;
        private static CustomSelectScrollController controller;

        private static SearchBy type;
        private static SearchCategory category;
        
        private void Awake()
        {
            var harmony = new Harmony(nameof(HS2_MakerSearch));
            harmony.PatchAll(typeof(HS2_MakerSearch));

            type = SearchBy.Name;
            category = SearchCategory.None;
        }

        public static void Search(string text)
        {
            if (category == SearchCategory.None)
                return;
            
            switch (category)
            {
                case SearchCategory.Hair:
                    cvsHair.UpdateHairList();
                    cvsHair.UpdateCustomUI();
                    break;
                case SearchCategory.Clothes:
                    cvsClothes.UpdateClothesList();
                    cvsClothes.UpdateCustomUI();
                    break;
            }

            if (text == "")
                return;
            
            var lowerText = text.ToLower();

            var trav = Traverse.Create(controller);
            var datas = trav.Field("scrollerDatas").GetValue<CustomSelectScrollController.ScrollData[]>();
            
            var datalist = datas.ToList();
            foreach (var data in datalist.ToArray())
            {
                var str = "";

                switch (type)
                {
                    case SearchBy.Name:
                        str = data.info.name;
                        AutoTranslator.Default.TranslateAsync(data.info.name, result => { str = result.Succeeded ? result.TranslatedText : data.info.name; });
                        break;
                    case SearchBy.AssetName:
                        str = data.info.assetName;
                        break;
                    case SearchBy.ID:
                        str = data.info.id.ToString();
                        break;
                }

                str = str.ToLower();

                if (str.Contains(lowerText)) 
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
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Start")]
        public static void CustomControl_Start_SetVars()
        {
            cvsHair = Singleton<CvsH_Hair>.Instance;
            cvsClothes = Singleton<CvsC_Clothes>.Instance;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        public static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            switch (no)
            {
                case 0:
                    category = SearchCategory.Face;
                    break;
                case 1:
                    category = SearchCategory.Body;
                    break;
                case 2:
                    category = SearchCategory.Hair;
                    controller = Traverse.Create(cvsHair).Field("sscHairType").GetValue<CustomSelectScrollController>();
                    break;
                case 3:
                    category = SearchCategory.Clothes;
                    controller = Traverse.Create(cvsClothes).Field("sscClothesType").GetValue<CustomSelectScrollController>();
                    break;
                case 4:
                    category = SearchCategory.Accessories;
                    break;
                case 5:
                    category = SearchCategory.Extra;
                    break;
            }

            if (category == SearchCategory.None)
                return;
            
            view = controller.GetComponent<LoopListView2>();
        }
        
        private enum SearchBy
        {
            Name,
            AssetName,
            ID
        }

        private enum SearchCategory
        {
            Face,
            Body,
            Hair,
            Clothes,
            Accessories,
            Extra,
            None
        }
    }
}