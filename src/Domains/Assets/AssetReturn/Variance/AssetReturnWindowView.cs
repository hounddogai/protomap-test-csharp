namespace Acme.Workforce.Assets.AssetReturn.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return window.</summary>
public sealed record AssetReturnWindowView(string Code, string Title, string Country, string CurrencyText, string RateText, string Version, string Ratio, string NameText, string Notes, string StatusLabel)
{
    public static AssetReturnWindowView ToAssetReturnWindowView(AssetReturnWindow item)
    {
        return new AssetReturnWindowView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Country,
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnWindowView> ToAssetReturnWindowViews(List<AssetReturnWindow> items)
    {
        var views = new List<AssetReturnWindowView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnWindowView(item));
        }
        return views;
    }

    public static double SumAssetReturnWindowView(AssetReturnWindowView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedVersion + parsedRatio;
    }
}
