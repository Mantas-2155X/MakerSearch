namespace HS2_MakerSearch
{
    public static class Tools
    {
        public static bool UpdateUI(SearchCategory category)
        {
            switch (category)
            {
                case SearchCategory.Face:
                    return false;
                case SearchCategory.Body:
                    return false;
                case SearchCategory.Hair:
                    HS2_MakerSearch.cvsHair.UpdateHairList();
                    HS2_MakerSearch.cvsHair.UpdateCustomUI();
                    break;
                case SearchCategory.Clothes:
                    HS2_MakerSearch.cvsClothes.UpdateClothesList();
                    HS2_MakerSearch.cvsClothes.UpdateCustomUI();
                    break;
                case SearchCategory.Accessories:
                    return false;
                case SearchCategory.Extra:
                    return false;
                case SearchCategory.None:
                    return false;
                default:
                    return false;
            }

            return true;
        }
        
        public enum SearchBy
        {
            Name,
            AssetName,
            ID
        }

        public enum SearchCategory
        {
            Face,
            Body,
            Hair,
            Clothes,
            Accessories,
            Extra,
            None
        }
    }
}