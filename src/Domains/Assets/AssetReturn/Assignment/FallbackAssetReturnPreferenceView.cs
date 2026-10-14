namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback asset return preference.</summary>
public sealed record FallbackAssetReturnPreferenceView(string Code, string Label, string TitleText, string Pinned, string Archived, string Location, string CapacityText, string AttemptsText, string Margin, string Version, string StatusLabel)
{
    public static FallbackAssetReturnPreferenceView ToFallbackAssetReturnPreferenceView(FallbackAssetReturnPreference item)
    {
        return new FallbackAssetReturnPreferenceView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Title, 120),
            $"{item.Pinned}",
            $"{item.Archived}",
            item.Location,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAssetReturnPreferenceView> ToFallbackAssetReturnPreferenceViews(List<FallbackAssetReturnPreference> items)
    {
        var views = new List<FallbackAssetReturnPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAssetReturnPreferenceView(item));
        }
        return views;
    }

    public static double SumFallbackAssetReturnPreferenceView(FallbackAssetReturnPreferenceView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedAttempts + parsedMargin;
    }
}
