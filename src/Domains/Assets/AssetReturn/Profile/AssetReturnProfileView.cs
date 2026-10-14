namespace Acme.Workforce.Assets.AssetReturn.Profile;

using System.Globalization;

/// <summary>How screens and exports show a asset return profile.</summary>
public sealed record AssetReturnProfileView(string Code, string TitleText, string Reviewer, string Margin, string DiscountText, string FteText, string CurrencyText, string AttemptsText, string UpdatedAt, string RatioText, string LocationText, string StatusLabel)
{
    public static AssetReturnProfileView ToAssetReturnProfileView(AssetReturnProfile item)
    {
        return new AssetReturnProfileView(
            item.Code,
            item.Title,
            item.Reviewer,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnProfileView> ToAssetReturnProfileViews(List<AssetReturnProfile> items)
    {
        var views = new List<AssetReturnProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnProfileView(item));
        }
        return views;
    }

    public static double SumAssetReturnProfileView(AssetReturnProfileView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedDiscount + parsedFte;
    }
}
