namespace Acme.Workforce.Assets.AssetReturn.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback asset return forecast.</summary>
public sealed record FallbackAssetReturnForecastView(string CodeText, string Name, string Approved, string DurationMinutes, string RatioText, string GraceDays, string Hours, string WeightText, string StatusLabel)
{
    public static FallbackAssetReturnForecastView ToFallbackAssetReturnForecastView(FallbackAssetReturnForecast item)
    {
        return new FallbackAssetReturnForecastView(
            item.Code,
            Text.Truncate(item.Name, 40),
            $"{item.Approved}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAssetReturnForecastView> ToFallbackAssetReturnForecastViews(List<FallbackAssetReturnForecast> items)
    {
        var views = new List<FallbackAssetReturnForecastView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAssetReturnForecastView(item));
        }
        return views;
    }

    public static double SumFallbackAssetReturnForecastView(FallbackAssetReturnForecastView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio + (double)parsedGraceDays;
    }
}
