using System;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using TMPro;
using ChaCustom;

using UnityEngine;
using Object = UnityEngine.Object;

namespace EC_MakerSearch
{
    public static class Tools
    {
        private static List<TMP_InputField> fields;
        public static readonly HashSet<CustomSelectInfo> disvisibleMemory = new HashSet<CustomSelectInfo>();
        public static readonly Dictionary<CustomSelectInfo, string> searchNameStrings = new Dictionary<CustomSelectInfo, string>();

        public static void CreateUI()
        {
            fields = new List<TMP_InputField>();
            
            var parent = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree");
            var inputField = parent.transform.Find("02_HairTop/tglFront/FrontTop/sldTemp/InputField");

            // Face
            {
                var faceObj = parent.transform.Find("00_FaceTop");
                var face = faceObj.GetComponent<CustomChangeFaceMenu>();

                var trav = Traverse.Create(face);
                
                var cvsItems = new []
                {
                    Singleton<CvsFaceAll>.Instance.gameObject,
                    trav.Field("cvsEyebrow").GetValue<CvsEyebrow>().gameObject,
                    trav.Field("cvsEye01").GetValue<CvsEye01>().gameObject,
                    trav.Field("cvsEye02").GetValue<CvsEye02>().gameObject,
                    Singleton<CvsNose>.Instance.gameObject,
                    trav.Field("cvsMouth").GetValue<CvsMouth>().gameObject,
                    trav.Field("cvsMole").GetValue<CvsMole>().gameObject,
                    trav.Field("cvsMakeup").GetValue<CvsMakeup>().gameObject
                };
                
                foreach (var ctrl in cvsItems)
                {
                    for (var i = 0; i < ctrl.transform.childCount; i++)
                    {
                        var child = ctrl.transform.GetChild(i);
                        
                        if(!child.name.Contains("win"))
                            continue;

                        if (!child.name.Contains("Kind") && !child.name.Contains("Layout")) 
                            continue;
                        
                        var window = child.Find("customSelectWindow");
                        SetupSearchBar(window, inputField);
                    }
                }
            }
            
            // Body
            {
                var bodyObj = parent.transform.Find("01_BodyTop");
                var body = bodyObj.GetComponent<CustomChangeBodyMenu>();

                var trav = Traverse.Create(body);
                
                var cvsItems = new []
                {
                    trav.Field("cvsBodyAll").GetValue<CvsBodyAll>().gameObject,
                    trav.Field("cvsBodyPaint").GetValue<CvsBodyPaint>().gameObject,
                    trav.Field("cvsBreast").GetValue<CvsBreast>().gameObject,
                    trav.Field("cvsSunburn").GetValue<CvsSunburn>().gameObject,
                    trav.Field("cvsUnderhair").GetValue<CvsUnderhair>().gameObject
                };
                
                foreach (var ctrl in cvsItems)
                {
                    for (var i = 0; i < ctrl.transform.childCount; i++)
                    {
                        var child = ctrl.transform.GetChild(i);
                        
                        if(!child.name.Contains("win"))
                            continue;

                        if (!child.name.Contains("Kind") && !child.name.Contains("Layout")) 
                            continue;
                        
                        var window = child.Find("customSelectWindow");
                        SetupSearchBar(window, inputField);
                    }
                }
            }
            
            // Hair
            {
                var hairObj = parent.transform.Find("02_HairTop");
                var hair = hairObj.GetComponent<CustomChangeHairMenu>();

                var cvsHair = Traverse.Create(hair).Field("cvsHair").GetValue<CvsHair[]>();
                
                foreach (var ctrl in cvsHair)
                {
                    var window = ctrl.transform.Find("winHairKind/customSelectWindow");
                    SetupSearchBar(window, inputField);
                }

                var cvsHairEtc = hairObj.Find("tglEtc/EtcTop").GetComponent<CvsHairEtc>();
                SetupSearchBar(cvsHairEtc.transform.Find("winGlossKind/customSelectWindow"), inputField);
            }
            
            // Clothes
            {
                var clothesObj = parent.transform.Find("03_ClothesTop");
                var clothes = clothesObj.GetComponent<CustomChangeClothesMenu>();

                var cvsClothes = Traverse.Create(clothes).Field("cvsClothes").GetValue<CvsClothes[]>();
                
                foreach (var ctrl in cvsClothes)
                {
                    for (var i = 0; i < ctrl.transform.childCount; i++)
                    {
                        var child = ctrl.transform.GetChild(i);
                        
                        if (!child.name.Contains("win") || !child.name.Contains("Kind"))
                            continue;

                        var window = child.Find("customSelectWindow");
                        SetupSearchBar(window, inputField);
                    }
                }
            }
            
            // Accessories
            {
                var accessoryObj = parent.transform.Find("04_AccessoryTop");
                var accessory = accessoryObj.GetComponent<CustomAcsChangeSlot>();

                var selectKinds = Traverse.Create(accessory).Field("customAcsSelectKind").GetValue<CustomAcsSelectKind[]>();
                
                foreach (var kind in selectKinds)
                {
                    var window = kind.transform.Find("customSelectWindow");
                    SetupSearchBar(window, inputField);
                }
            }
        }

        private static void SetupSearchBar(Transform window, Transform inputField)
        {
            var bg = window.Find("BasePanel/imgWindowBack");
            bg.GetComponent<RectTransform>().offsetMin = new Vector2(0, -10);

            var newInputField = Object.Instantiate(inputField.gameObject, window);
            newInputField.name = "Search";

            var rect = newInputField.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(-390, 0);
            rect.offsetMax = new Vector2(-10, -610);

            var input = newInputField.GetComponent<TMP_InputField>();
            input.contentType = TMP_InputField.ContentType.Standard;
            input.characterLimit = 64;
                    
            input.text = "";
            input.textComponent.text = "";

            input.characterValidation = TMP_InputField.CharacterValidation.None;
            input.keyboardType = TouchScreenKeyboardType.Default;

            var placeholder = newInputField.transform.Find("Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
            placeholder.text = "Search";
            
            input.onValueChanged.RemoveAllListeners();
            input.onEndEdit.RemoveAllListeners();

            input.onValueChanged.AddListener(delegate(string text) { placeholder.enabled = text == ""; });
            input.onEndEdit.AddListener(delegate(string text)
            {
                EC_MakerSearch.searchString = text;
                EC_MakerSearch.Search();
            });
            
            fields.Add(input);
        }
        
        public static bool ItemMatchesSearch(CustomSelectInfo data, string searchStr)
        {
            var searchIn = "";

            switch (EC_MakerSearch.searchBy.Value)
            {
                case SearchBy.Name:
                    searchIn = data.name;
                    
                    if (EC_MakerSearch.useTranslatedCache.Value)
                        searchIn = searchNameStrings.TryGetValue(data, out var cachedTranslation) ? cachedTranslation : data.name;

                    break;
                case SearchBy.AssetBundle:
                    searchIn = data.assetBundle;
                    break;
            }

            var rule = StringComparison.Ordinal;
            if (!EC_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }

            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
        
        public static void ResetSearch()
        {
            if (EC_MakerSearch.ctrl == null || fields == null || fields.Count < 1)
                return;
            
            ResetDisables();
            
            if (EC_MakerSearch.searchString == "") 
                return;
            
            EC_MakerSearch.searchString = "";

            foreach (var field in fields.Where(field => field != null))
                field.text = "";
        }

        public static void ResetDisables()
        {
            var trav = Traverse.Create(EC_MakerSearch.ctrl);
            var datas = trav.Field("lstSelectInfo").GetValue<List<CustomSelectInfo>>();

            foreach (var t in datas)
                EC_MakerSearch.ctrl.DisvisibleItem(t.index, disvisibleMemory.Contains(t));
        }
        
        public static void RememberDisvisibles()
        {
            disvisibleMemory.Clear();
            
            var trav = Traverse.Create(EC_MakerSearch.ctrl);
            var datas = trav.Field("lstSelectInfo").GetValue<List<CustomSelectInfo>>();

            foreach (var t in datas.Where(t => t.disvisible))
                disvisibleMemory.Add(t);
        }
        
        public enum SearchBy
        {
            Name,
            AssetBundle
        }
    }
}