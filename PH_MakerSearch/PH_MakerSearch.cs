using System.Linq;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

using UnityEngine.UI;

namespace PH_MakerSearch
{
    [BepInProcess("PlayHome64bit")]
    [BepInProcess("PlayHome32bit")] // *tumbleweed*
    [BepInPlugin(nameof(PH_MakerSearch), nameof(PH_MakerSearch), VERSION)]
    public class PH_MakerSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.1.1";

        public static string searchString;
        
        public static InputField input;
        public static ThumbnailSelectUI selectUI;

        public static ConfigEntry<bool> caseSensitive { get; private set; }
        public static ConfigEntry<bool> useTranslatedCache { get; private set; }

        private void Awake()
        {
            caseSensitive = Config.Bind(new ConfigDefinition("General", "Case sensitive"), false);
            useTranslatedCache = Config.Bind(new ConfigDefinition("General", "Search translated cache"), true, new ConfigDescription("Search in translated cache, if nonexistant then translate. Only works when search includes name"));

            var harmony = new Harmony(nameof(PH_MakerSearch));
            harmony.PatchAll(typeof(Hooks));
        }
        
        public static void Search()
        {
            Tools.ResetDisables();

            if (searchString == "")
                return;

            var trav = Traverse.Create(selectUI);
            var datas = trav.Field("cells").GetValue<ThumbnailSelectCell[]>();

            foreach (var data in datas.Where(data => !Tools.ItemMatchesSearch(data, searchString)))
                data.gameObject.SetActive(false);
        }
    }
}