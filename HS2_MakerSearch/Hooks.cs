using HarmonyLib;

using AIChara;
using CharaCustom;
using SuperScrollView;

namespace HS2_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_SetVars(CustomControl __instance)
        {
            HS2_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            HS2_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
            HS2_MakerSearch.cvsAccessories = Singleton<CvsA_Slot>.Instance;
            HS2_MakerSearch.cvsSkin = Singleton<CvsB_Skin>.Instance;
            HS2_MakerSearch.cvsSunburn = Singleton<CvsB_Sunburn>.Instance;
            HS2_MakerSearch.cvsNip = Singleton<CvsB_Nip>.Instance;
            HS2_MakerSearch.cvsUnderhair = Singleton<CvsB_Underhair>.Instance;
            HS2_MakerSearch.cvsPaint = Singleton<CvsB_Paint>.Instance;
            
            HS2_MakerSearch.sex = Traverse.Create(__instance).Property("chaCtrl").Property("sex").GetValue<byte>();
            
            Tools.CreateUI();

            // Switch between body Skin and Detail
            HS2_MakerSearch.cvsSkin.items[0].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        private static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            Tools.ResetSearch();
            
            switch (no)
            {
                case 0:
                    HS2_MakerSearch.category = Tools.SearchCategory.Face;
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
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsAccessories).Field("sscAcs").GetValue<CustomSelectScrollController>();
                    break;
                case 5:
                    HS2_MakerSearch.category = Tools.SearchCategory.Extra;
                    return;
                default:
                    HS2_MakerSearch.category = Tools.SearchCategory.None;
                    return;
            }

            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Skin), "ChangeMenuFunc")]
        private static void CvsB_Skin_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            switch (HS2_MakerSearch.cvsSkin.GetSelectTab())
            {
                case 0:
                    HS2_MakerSearch.category = Tools.SearchCategory.BodySkin;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscSkinType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 1:
                    HS2_MakerSearch.category = Tools.SearchCategory.BodyDetail;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscDetailType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Sunburn), "ChangeMenuFunc")]
        private static void CvsB_Sunburn_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscSunburnType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodySunburn;
            HS2_MakerSearch.controller = ___sscSunburnType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Nip), "ChangeMenuFunc")]
        private static void CvsB_Nip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscNipType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyNip;
            HS2_MakerSearch.controller = ___sscNipType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Underhair), "ChangeMenuFunc")]
        private static void CvsB_Underhair_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscUnderhairType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyUnderhair;
            HS2_MakerSearch.controller = ___sscUnderhairType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Paint), "ChangeMenuFunc")]
        private static void CvsB_Paint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyPaint;
            HS2_MakerSearch.controller = ___sscPaintType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
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