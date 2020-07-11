using HarmonyLib;

using ChaCustom;

namespace KK_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        public static void CustomControl_Initialize_CreateUI()
        {
            Tools.CreateUI();
        }
    }
}