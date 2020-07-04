using HarmonyLib;

using CharaCustom;
using SuperScrollView;

namespace HS2_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Start")]
        public static void CustomControl_Start_SetVars()
        {
            HS2_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            HS2_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        public static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            switch (no)
            {
                case 0:
                    HS2_MakerSearch.category = Tools.SearchCategory.Face;
                    return;
                case 1:
                    HS2_MakerSearch.category = Tools.SearchCategory.Body;
                    return;
                case 2:
                    HS2_MakerSearch.category = Tools.SearchCategory.Hair;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsHair).Field("sscHairType").GetValue<CustomSelectScrollController>();
                    break;
                case 3:
                    HS2_MakerSearch.category = Tools.SearchCategory.Clothes;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsClothes).Field("sscClothesType").GetValue<CustomSelectScrollController>();
                    break;
                case 4:
                    HS2_MakerSearch.category = Tools.SearchCategory.Accessories;
                    return;
                case 5:
                    HS2_MakerSearch.category = Tools.SearchCategory.Extra;
                    return;
                default:
                    HS2_MakerSearch.category = Tools.SearchCategory.None;
                    return;
            }
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
    }
}