using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PH_MakerSearch
{
    public static class Tools
    {
        public static readonly Dictionary<string, string> searchNameStrings = new Dictionary<string, string>();
        
        public static void CreateUI(EditMode mode, MoveableThumbnailSelectUI itemSelectUI)
        {
            if(mode == null || itemSelectUI == null)
                return;
            
            PH_MakerSearch.selectUI = Traverse.Create(itemSelectUI).Field("select").GetValue<ThumbnailSelectUI>();
            
            var window = PH_MakerSearch.selectUI.transform.Find("Scroll View");
            var inputField = mode.transform.Find("Canvas/Body/Mains/General/CustomSliderUI(Clone)/InputField");
            var resetButton = mode.transform.Find("Canvas/Originals/Button");

            var content = window.transform.Find("Viewport");
            var cRect = content.GetComponent<RectTransform>();
            cRect.offsetMin = new Vector2(cRect.offsetMin.x, 31);
            
            var newInputField = Object.Instantiate(inputField.gameObject, window);
            newInputField.name = "Search";

            var rect = newInputField.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(-217, -286);
            rect.offsetMax = new Vector2(-75, -261);

            var resetCopy = Object.Instantiate(resetButton, window);
            resetCopy.name = "Reset";
            
            Object.Destroy(resetCopy.GetComponent<LayoutElement>());
            
            var resetRect = resetCopy.GetComponent<RectTransform>();
            resetRect.offsetMin = new Vector2(218, -578);
            resetRect.offsetMax = new Vector2(60, 60);
            
            var bg = resetCopy.Find("Background");
            
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.offsetMin = new Vector2(89, -319);
            bgRect.offsetMax = new Vector2(139, -290);
            
            var resetText = resetCopy.GetComponentInChildren<Text>();
            resetText.text = "Reset";
            
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

            var button = resetCopy.GetComponent<Button>();

            var oldColors = button.colors;
            button.colors = new ColorBlock
            {
                colorMultiplier = oldColors.colorMultiplier,
                disabledColor = oldColors.disabledColor,
                fadeDuration = oldColors.fadeDuration,
                highlightedColor = new Color(0.191f, 1, 0.933f, 0.5f),
                normalColor = new Color(0.191f, 1, 0.933f, 0),
                pressedColor = oldColors.pressedColor
            };

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate
            {
                input.text = "";
                PH_MakerSearch.searchString = "";
                PH_MakerSearch.Search();
            });

            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.offsetMax = new Vector2(60, 60);
            
            PH_MakerSearch.input = input;
        }
        
        public static bool ItemMatchesSearch(string searchStr, int idx)
        {
            var searchIn = Traverse.Create(PH_MakerSearch.selectUI).Field("datas").GetValue<List<CustomSelectSet>>()[idx].name;

            if (PH_MakerSearch.useTranslatedCache.Value)
                searchIn = searchNameStrings.TryGetValue(searchIn, out var cachedTranslation) ? cachedTranslation : searchIn;

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