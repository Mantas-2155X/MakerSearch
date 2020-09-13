using HarmonyLib;

using ChaCustom;
using UnityEngine;

namespace EC_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            if (EC_MakerSearch.DisableHiddenTabs)
                EC_MakerSearch.acsGrp = Traverse.Create(Singleton<CustomAcsChangeSlot>.Instance).Field("cgAccessoryTop").GetValue<CanvasGroup>();

            EC_MakerSearch.ctrl = null;
            
            Tools.disvisibleMemory.Clear();
            Tools.CreateUI();
        }

        // Thanks "MakerOptimizations DisableHiddenTabs" for giving me a headache
        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Update")]
        private static void CustomSelectListCtrl_Update_ChangeController(CustomSelectListCtrl __instance)
        {
            if (!EC_MakerSearch.DisableHiddenTabs)
                return;
            
            if (EC_MakerSearch.ctrl == __instance) 
                return;

            if (__instance.canvasGrp == null || __instance.canvasGrp.Length == 0)
                return;

            var win = __instance.canvasGrp[0];
            
            if (!win.name.Contains("win") || !win.interactable)
                return;

            if (win.name.Contains("Acs") && (EC_MakerSearch.acsGrp == null || !EC_MakerSearch.acsGrp.interactable))
                return;
            
            Tools.ResetSearch();
            
            EC_MakerSearch.ctrl = __instance;
            Tools.RememberDisvisibles();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "UpdateStateNew")]
        private static void CustomSelectListCtrl_UpdateStateNew_ChangeController(CustomSelectListCtrl __instance)
        {
            Tools.ResetSearch();

            if (!__instance.canvasGrp[0].interactable) 
                return;
            
            EC_MakerSearch.ctrl = __instance;
            Tools.RememberDisvisibles();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomAcsSelectKind), "UpdateCustomUI")]
        private static void Custom_Acs_SelectKind_UpdateCustomUI_ChangeController(CustomSelectListCtrl ___listCtrl)
        {
            Tools.ResetSearch();
            
            EC_MakerSearch.ctrl = ___listCtrl;
            Tools.RememberDisvisibles();
        }
    }
}