namespace Acme.Workforce.Assets.AssetReturn.Profile;

using System.Globalization;

/// <summary>How screens and exports show a asset return forecast.</summary>
public sealed record AssetReturnForecastView(string Code, string Name, string Cost, string StartsAtText, string Notes, string StatusLabel)
{
    public static AssetReturnForecastView ToAssetReturnForecastView(AssetReturnForecast item)
    {
        return new AssetReturnForecastView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnForecastView> ToAssetReturnForecastViews(List<AssetReturnForecast> items)
    {
        var views = new List<AssetReturnForecastView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnForecastView(item));
        }
        return views;
    }

    public static double SumAssetReturnForecastView(AssetReturnForecastView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedStartsAt;
    }
}
