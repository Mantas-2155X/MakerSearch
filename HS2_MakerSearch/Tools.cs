using System;
using System.Linq;

using AIChara;
using CharaCustom;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;
using XUnity.AutoTranslator.Plugin.Core;

namespace HS2_MakerSearch
{
    public static class Tools
    {
        public static readonly InputField[] fields = new InputField[9];
        
        private static readonly string[] targets =
        {
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/H_Hair/Setting/Setting01",                      // Hair
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01",     // Clothes
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinAccessory/A_Slot/Setting/Setting01",                 // Accessories
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting01",                      // Body Skin
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting02",                      // Body Detail
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Sunburn/Setting/Setting01",                   // Body Sunburn
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Nip/Setting/Setting01",                       // Body Nip
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Underhair/Setting/Setting01",                 // Body Underhair
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Paint/Setting/Setting01",                     // Body Paint
        };
 
        public static void CreateUI()
        {
            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_ShapeWhole/Scroll View/Viewport/Content/SliderSet/SldInputField");

            var i = 0;
            foreach (var targetStr in targets)
            {
                var target = GameObject.Find(targetStr);
                
                var cp = UnityEngine.Object.Instantiate(orig, target.transform);
                cp.name = "Search";

                var placeholderObj = cp.transform.Find("Placeholder");
                var placeholder = placeholderObj.GetComponent<Text>();
                placeholder.text = "Search";

                UnityEngine.Object.Destroy(cp.transform.Find("SldInputField Input Caret"));

                var rect = cp.GetComponent<RectTransform>();

                if (i == 1) // Clothes
                {
                    rect.offsetMin = new Vector2(-250, 3);
                    rect.offsetMax = new Vector2(0, -383);
                } 
                else
                {
                    rect.offsetMin = new Vector2(-420, 3);
                    rect.offsetMax = new Vector2(0, -383);

                    var box = target.transform.Find("SelectBox");
                    var scrollview = box.Find("Scroll View");
                    
                    box.GetComponent<RectTransform>().offsetMin = new Vector2(0, -372);
                    scrollview.GetComponent<RectTransform>().offsetMin = new Vector2(0, i == 2 ? -264 : i == 4 ? -332 : -372);
                }

                var input = cp.GetComponent<InputField>();
                input.contentType = InputField.ContentType.Standard;
                input.characterLimit = 64;

                input.onValueChanged.RemoveAllListeners();
                input.onEndEdit.RemoveAllListeners();

                input.textComponent.text = "";
                input.text = "";

                input.onValueChanged.AddListener(delegate(string text) { placeholder.enabled = text == ""; });
                input.onEndEdit.AddListener(delegate(string text)
                {
                    HS2_MakerSearch.searchString = text;
                    HS2_MakerSearch.Search();
                });

                fields[i] = input;

                if (i == 2)
                    cp.SetActive(false);
                
                i++;
            }
        }

        public static bool ItemMatchesSearch(CustomSelectInfo data, string searchStr)
        {
            var searchIn = "";

            switch (HS2_MakerSearch.searchBy.Value)
            {
                case SearchBy.Name:
                    searchIn = data.name;
                    
                    if (HS2_MakerSearch.useTranslatedCache.Value)
                        AutoTranslator.Default.TranslateAsync(data.name, result => { searchIn = result.Succeeded ? result.TranslatedText : data.name; });

                    break;
                case SearchBy.AssetBundle:
                    searchIn = data.assetBundle;
                    break;
            }

            var rule = StringComparison.Ordinal;
            if (!HS2_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }

            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
        
        public static bool UpdateUI(SearchCategory category)
        {
            switch (category)
            {
                case SearchCategory.Face:
                    return false;
                case SearchCategory.BodySkin:
                    var listBSkin = CvsBase.CreateSelectList(HS2_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_skin_b : ChaListDefine.CategoryNo.ft_skin_b);
                    Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscSkinType").Method("CreateList", listBSkin).GetValue();

                    HS2_MakerSearch.cvsSkin.UpdateCustomUI();
                    break;
                case SearchCategory.BodyDetail:
                    var listBDetail = CvsBase.CreateSelectList(HS2_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_detail_b : ChaListDefine.CategoryNo.ft_detail_b);
                    Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscDetailType").Method("CreateList", listBDetail).GetValue();

                    HS2_MakerSearch.cvsSkin.UpdateCustomUI();
                    break;
                case SearchCategory.BodySunburn:
                    var listBSunburn = CvsBase.CreateSelectList(HS2_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_sunburn : ChaListDefine.CategoryNo.ft_sunburn);
                    Traverse.Create(HS2_MakerSearch.cvsSunburn).Field("sscSunburnType").Method("CreateList", listBSunburn).GetValue();

                    HS2_MakerSearch.cvsSunburn.UpdateCustomUI();
                    break;
                case SearchCategory.BodyNip:
                    var listBNip = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_nip);
                    Traverse.Create(HS2_MakerSearch.cvsNip).Field("sscNipType").Method("CreateList", listBNip).GetValue();

                    HS2_MakerSearch.cvsNip.UpdateCustomUI();
                    break;
                case SearchCategory.BodyUnderhair:
                    var listBUnderhair = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_underhair);
                    Traverse.Create(HS2_MakerSearch.cvsUnderhair).Field("sscUnderhairType").Method("CreateList", listBUnderhair).GetValue();

                    HS2_MakerSearch.cvsUnderhair.UpdateCustomUI();
                    break;
                case SearchCategory.BodyPaint:
                    var listBPaint = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint);
                    Traverse.Create(HS2_MakerSearch.cvsPaint).Field("sscPaintType").Method("CreateList", listBPaint).GetValue();

                    HS2_MakerSearch.cvsPaint.UpdateCustomUI();
                    break;
                case SearchCategory.Hair:
                    HS2_MakerSearch.cvsHair.UpdateHairList();
                    HS2_MakerSearch.cvsHair.UpdateCustomUI();
                    break;
                case SearchCategory.Clothes:
                    HS2_MakerSearch.cvsClothes.UpdateClothesList();
                    HS2_MakerSearch.cvsClothes.UpdateCustomUI();
                    break;
                case SearchCategory.Accessories:
                    HS2_MakerSearch.cvsAccessories.UpdateAcsList();
                    HS2_MakerSearch.cvsAccessories.UpdateCustomUI();
                    break;
                case SearchCategory.Extra:
                    return false;
                case SearchCategory.None:
                    return false;
                default:
                    return false;
            }

            return true;
        }

        public static void ResetSearch()
        {
            if (HS2_MakerSearch.searchString == "") 
                return;
            
            UpdateUI(HS2_MakerSearch.category);
            HS2_MakerSearch.searchString = "";

            foreach (var field in fields.Where(field => field != null))
                field.text = "";
        }
        
        public enum SearchBy
        {
            Name,
            AssetBundle
        }

        public enum SearchCategory
        {
            Face,
            BodySkin,
            BodyDetail,
            BodySunburn,
            BodyNip,
            BodyUnderhair,
            BodyPaint,
            Hair,
            Clothes,
            Accessories,
            Extra,
            None
        }
    }
}