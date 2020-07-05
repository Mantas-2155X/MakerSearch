using HarmonyLib;

using AIChara;
using CharaCustom;
using SuperScrollView;

namespace AI_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Start")]
        public static void CustomControl_Start_SetVars()
        {
            AI_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            AI_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
            AI_MakerSearch.cvsAccessories = Singleton<CvsA_Slot>.Instance;
            
            Tools.CreateUI();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        public static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            Tools.ResetSearch();
            
            switch (no)
            {
                case 0:
                    AI_MakerSearch.category = Tools.SearchCategory.Face;
                    return;
                case 1:
                    AI_MakerSearch.category = Tools.SearchCategory.Body;
                    return;
                case 2:
                    AI_MakerSearch.category = Tools.SearchCategory.Hair;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsHair).Field("sscHairType").GetValue<CustomSelectScrollController>();
                    break;
                case 3:
                    AI_MakerSearch.category = Tools.SearchCategory.Clothes;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsClothes).Field("sscClothesType").GetValue<CustomSelectScrollController>();
                    break;
                case 4:
                    AI_MakerSearch.category = Tools.SearchCategory.Accessories;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsAccessories).Field("sscAcs").GetValue<CustomSelectScrollController>();
                    break;
                case 5:
                    AI_MakerSearch.category = Tools.SearchCategory.Extra;
                    return;
                default:
                    AI_MakerSearch.category = Tools.SearchCategory.None;
                    return;
            }

            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsBase), "ChangeMenuFunc")]
        public static void CvsBase_ChangeMenuFunc_ResetSearch() => Tools.ResetSearch();
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeMenuFunc")]
        public static void CvsA_Slot_ChangeMenuFunc_ResetSearch(CvsA_Slot __instance)
        {
            var nowAcs = Traverse.Create(__instance).Property("nowAcs").GetValue<ChaFileAccessory>();
            
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(nowAcs.parts[__instance.SNo].type != 350);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeAcsType")]
        public static void CvsA_Slot_ChangeAcsType_ResetSearch(int idx)
        {
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(idx != 0);

            Tools.ResetSearch();
        }
    }
}