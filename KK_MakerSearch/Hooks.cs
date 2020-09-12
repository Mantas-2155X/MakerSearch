using HarmonyLib;

using ChaCustom;

namespace KK_MakerSearch
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

            if (!__instance.canvasGrp[0].interactable) 
                return;
            
            KK_MakerSearch.ctrl = __instance;
            Tools.RememberDisvisibles();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomAcsSelectKind), "UpdateCustomUI")]
        private static void CustomAcsSelectKind_UpdateCustomUI_ChangeController(CustomSelectListCtrl ___listCtrl)
        {
            Tools.ResetSearch();
            
            KK_MakerSearch.ctrl = ___listCtrl;
            Tools.RememberDisvisibles();
        }
    }
}