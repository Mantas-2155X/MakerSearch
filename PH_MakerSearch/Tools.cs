using System;
using System.Linq;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using XUnity.AutoTranslator.Plugin.Core;

namespace PH_MakerSearch
{
    public static class Tools
    {
        public static void CreateUI(EditMode mode, MoveableThumbnailSelectUI itemSelectUI)
        {
            if(mode == null || itemSelectUI == null)
                return;
            
            PH_MakerSearch.selectUI = Traverse.Create(itemSelectUI).Field("select").GetValue<ThumbnailSelectUI>();
            
            var window = PH_MakerSearch.selectUI.transform.Find("Scroll View");
            var inputField = mode.transform.Find("Canvas/Body/Mains/General/CustomSliderUI(Clone)/InputField");

            var content = window.transform.Find("Viewport");
            var cRect = content.GetComponent<RectTransform>();
            cRect.offsetMin = new Vector2(cRect.offsetMin.x, 31);
            
            var newInputField = UnityEngine.Object.Instantiate(inputField.gameObject, window);
            newInputField.name = "Search";

            var rect = newInputField.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(-217, -286);
            rect.offsetMax = new Vector2(-23, -261);

            var input = newInputField.GetComponent<InputField>();
            input.contentType = InputField.ContentType.Standard;
            input.characterLimit = 64;
                    
            input.text = "";
            input.textComponent.text = "";

            input.characterValidation = InputField.CharacterValidation.None;
            input.keyboardType = TouchScreenKeyboardType.Default;

            foreach (var text in newInputField.GetComponentsInChildren<Text>())
                text.alignment = TextAnchor.MiddleCenter;
            
            var placeholder = newInputField.transform.Find("Placeholder").GetComponent<Text>();
            placeholder.text = "Search";
            
            input.onValueChanged.RemoveAllListeners();
            input.onEndEdit.RemoveAllListeners();

            input.onValueChanged.AddListener(delegate(string text) { placeholder.enabled = text == ""; });
            input.onEndEdit.AddListener(delegate(string text)
            {
                PH_MakerSearch.searchString = text;
                PH_MakerSearch.Search();
            });

            PH_MakerSearch.input = input;
        }
        
        public static bool ItemMatchesSearch(ThumbnailSelectCell data, string searchStr)
        {
            var searchIn = Traverse.Create(data).Field("nameText").GetValue<Text>().text;

            if (PH_MakerSearch.useTranslatedCache.Value)
                TranslationHelper.Translate(data.name, s => { searchIn = s; });

            var rule = StringComparison.Ordinal;
            if (!PH_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }

            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
        
        public static void ResetSearch()
        {
            ResetDisables();
            
            PH_MakerSearch.searchString = "";
            PH_MakerSearch.input.text = "";
        }
        
        public static void ResetDisables()
        {
            if (PH_MakerSearch.selectUI == null)
                return;
            
            PH_MakerSearch.selectUI.UpdateEnables();
        }
    }
}