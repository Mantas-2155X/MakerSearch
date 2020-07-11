using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using ChaCustom;
using TMPro;

using UnityEngine;

namespace KK_MakerSearch
{
    public static class Tools
    {
        private static List<TMP_InputField> fields;

        public static void CreateUI()
        {
            fields = new List<TMP_InputField>();
            
            var parent = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree");
            var inputField = parent.transform.Find("02_HairTop/tglFront/FrontTop/sldTemp/InputField");

            // Hair
            {
                var hairObj = parent.transform.Find("02_HairTop");
                var hair = hairObj.GetComponent<CustomChangeHairMenu>();

                var cvsHair = Traverse.Create(hair).Field("cvsHair").GetValue<CvsHair[]>();
                
                foreach (var ctrl in cvsHair)
                {
                    var window = ctrl.transform.Find("winHairKind/customSelectWindow");
                    SetupSearchBar(window, inputField, ctrl);
                }

                var cvsHairEtc = hairObj.Find("tglEtc/EtcTop").GetComponent<CvsHairEtc>();
                SetupSearchBar(cvsHairEtc.transform.Find("winGlossKind/customSelectWindow"), inputField, cvsHairEtc);
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
                        
                        if(!child.name.Contains("win") && !child.name.Contains("Kind"))
                            continue;

                        var window = child.Find("customSelectWindow");
                        SetupSearchBar(window, inputField, ctrl);
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
                    SetupSearchBar(window, inputField, kind);
                }
            }
        }

        private static void SetupSearchBar(Transform window, Transform inputField, object ctrl)
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
                KK_MakerSearch.searchString = text;
                //KK_MakerSearch.Search();
                
                //> var ctrl = geti<ChaCustom.CustomSelectListCtrl>() 
                //> var act = ctrl.onChangeItemFunc
                //> ctrl.Create(act)
            });
            
            fields.Add(input);
        }
        
        public static void ResetSearch()
        {
            if (KK_MakerSearch.searchString == "") 
                return;
            
            //UpdateUI(AI_MakerSearch.category);
            KK_MakerSearch.searchString = "";

            foreach (var field in fields.Where(field => field != null))
                field.text = "";
        }
    }
}