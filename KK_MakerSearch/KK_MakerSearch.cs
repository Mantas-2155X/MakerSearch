using BepInEx;

using HarmonyLib;

namespace KK_MakerSearch
{
    [BepInProcess("Koikatu")]
    [BepInPlugin(nameof(KK_MakerSearch), nameof(KK_MakerSearch), VERSION)]
    public class KK_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";

        public static string searchString;
        
        private void Awake()
        {
            var harmony = new Harmony(nameof(KK_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
        }
    }
}