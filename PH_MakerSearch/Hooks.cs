using HarmonyLib;

namespace PH_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EditMode), "Setup")]
        private static void EditMode_Setup_CreateUI(EditMode __instance, MoveableThumbnailSelectUI ___itemSelectUI)
        {
            Tools.CreateUI(__instance, ___itemSelectUI);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(MoveableThumbnailSelectUI), "UpdateEnables")]
        private static void MoveableThumbnailSelectUI_UpdateEnables_ResetSearch(ThumbnailSelectUI ___select)
        {
            if (___select == PH_MakerSearch.selectUI)
                Tools.ResetSearch();
        }
    }
}