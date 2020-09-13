using System.Collections.Generic;
using System.Threading;
using HarmonyLib;

namespace PH_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EditMode), "Setup")]
        private static void EditMode_Setup_CreateUI(EditMode __instance, MoveableThumbnailSelectUI ___itemSelectUI)
        {
            Tools.searchNameStrings.Clear();
            Tools.CreateUI(__instance, ___itemSelectUI);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(ThumbnailSelectUI), "SetupCells")]
        private static void CustomSelectListCtrl_SetupCells_GetSelectInfos(ThumbnailSelectCell[] ___cells, List<CustomSelectSet> ___datas)
        {
            if (___cells == null)
                return;

            var t = new Thread(TranslateItems)
            {
                IsBackground = true,
                Name = "Translate items",
                Priority = ThreadPriority.BelowNormal
            };
            
            t.Start();

            void TranslateItems()
            {
                for (var i = 0; i < ___datas.Count; i++)
                {
                    if(Tools.searchNameStrings.ContainsKey(___cells[i]))
                        continue;

                    var idx = i;
                    TranslationHelper.Translate(___datas[i].name, s => Tools.searchNameStrings[___cells[idx]] = ___datas[idx].name + "/v" + s);
                }
            }
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(MoveableThumbnailSelectUI), "UpdateEnables")]
        private static void MoveableThumbnailSelectUI_UpdateEnables_ResetSearch(ThumbnailSelectUI ___select)
        {
            if (___select == PH_MakerSearch.selectUI)
                Tools.ResetSearch();
        }
    }
}