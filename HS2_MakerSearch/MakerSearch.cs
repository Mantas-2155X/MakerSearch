using System.Linq;

using BepInEx;

using HarmonyLib;

using CharaCustom;
using SuperScrollView;

namespace HS2_MakerSearch
{
    [BepInPlugin(nameof(HS2_MakerSearch), nameof(HS2_MakerSearch), VERSION)]
    public class HS2_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";
        
        private static CvsH_Hair cvsHair;
        
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
                    cvsHair.UpdateCustomUI();
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
            
            var num = datas.Length / trav.Field("countPerRow").GetValue<int>();
            view.ReSetListItemCount(num);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsH_Hair), "Start")]
        public static void CvsH_Hair_Start_SetVars(CvsH_Hair __instance)
        {
            cvsHair = __instance;

            var trav = Traverse.Create(cvsHair);
            controller = trav.Field("sscHairType").GetValue<CustomSelectScrollController>();
            
            view = controller.GetComponent<LoopListView2>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        public static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            switch (no)
            {
                case 0:
                    category = SearchCategory.None;
                    break;
                case 1:
                    category = SearchCategory.None;
                    break;
                case 2:
                    category = SearchCategory.Hair;
                    break;
                case 3:
                    category = SearchCategory.None;
                    break;
                case 4:
                    category = SearchCategory.None;
                    break;
                case 5:
                    category = SearchCategory.None;
                    break;
            }
        }
        
        private enum SearchBy
        {
            Name,
            AssetName,
            ID
        }

        private enum SearchCategory
        {
            None,
            Hair
        }
    }
}