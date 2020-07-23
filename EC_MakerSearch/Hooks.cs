using HarmonyLib;

using ChaCustom;

namespace EC_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            Tools.CreateUI();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "UpdateStateNew")]
        private static void CustomSelectListCtrl_UpdateStateNew_ChangeController(CustomSelectListCtrl __instance)
        {
            Tools.ResetSearch();
            
            if(__instance.canvasGrp[0].interactable)
                EC_MakerSearch.ctrl = __instance;
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomAcsSelectKind), "UpdateCustomUI")]
        private static void CustomAcsSelectKind_UpdateCustomUI_ChangeController(CustomSelectListCtrl ___listCtrl)
        {
            Tools.ResetSearch();
            
            EC_MakerSearch.ctrl = ___listCtrl;
        }
    }
}