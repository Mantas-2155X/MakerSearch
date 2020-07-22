using HarmonyLib;

using AIChara;
using CharaCustom;
using SuperScrollView;

namespace AI_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_SetVars(CustomControl __instance)
        {
            AI_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            AI_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
            AI_MakerSearch.cvsAccessories = Singleton<CvsA_Slot>.Instance;
            AI_MakerSearch.cvsSkin = Singleton<CvsB_Skin>.Instance;
            AI_MakerSearch.cvsSunburn = Singleton<CvsB_Sunburn>.Instance;
            AI_MakerSearch.cvsNip = Singleton<CvsB_Nip>.Instance;
            AI_MakerSearch.cvsUnderhair = Singleton<CvsB_Underhair>.Instance;
            AI_MakerSearch.cvsPaint = Singleton<CvsB_Paint>.Instance;
            
            AI_MakerSearch.sex = Traverse.Create(__instance).Property("chaCtrl").Property("sex").GetValue<byte>();
            
            Tools.CreateUI();

            // Switch between body Skin and Detail
            AI_MakerSearch.cvsSkin.items[0].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        private static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            Tools.ResetSearch();
            
            switch (no)
            {
                case 0:
                    AI_MakerSearch.category = Tools.SearchCategory.Face;
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
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Skin), "ChangeMenuFunc")]
        private static void CvsB_Skin_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            switch (AI_MakerSearch.cvsSkin.GetSelectTab())
            {
                case 0:
                    AI_MakerSearch.category = Tools.SearchCategory.BodySkin;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscSkinType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 1:
                    AI_MakerSearch.category = Tools.SearchCategory.BodyDetail;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscDetailType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Sunburn), "ChangeMenuFunc")]
        private static void CvsB_Sunburn_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscSunburnType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodySunburn;
            AI_MakerSearch.controller = ___sscSunburnType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Nip), "ChangeMenuFunc")]
        private static void CvsB_Nip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscNipType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyNip;
            AI_MakerSearch.controller = ___sscNipType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Underhair), "ChangeMenuFunc")]
        private static void CvsB_Underhair_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscUnderhairType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyUnderhair;
            AI_MakerSearch.controller = ___sscUnderhairType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Paint), "ChangeMenuFunc")]
        private static void CvsB_Paint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyPaint;
            AI_MakerSearch.controller = ___sscPaintType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsBase), "ChangeMenuFunc")]
        private static void CvsBase_ChangeMenuFunc_ResetSearch() => Tools.ResetSearch();
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeMenuFunc")]
        private static void CvsA_Slot_ChangeMenuFunc_ResetSearch(CvsA_Slot __instance)
        {
            var nowAcs = Traverse.Create(__instance).Property("nowAcs").GetValue<ChaFileAccessory>();
            
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(nowAcs.parts[__instance.SNo].type != 350);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeAcsType")]
        private static void CvsA_Slot_ChangeAcsType_ResetSearch(int idx)
        {
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(idx != 0);

            Tools.ResetSearch();
        }
    }
}