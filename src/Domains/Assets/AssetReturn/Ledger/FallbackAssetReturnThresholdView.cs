namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback asset return threshold.</summary>
public sealed record FallbackAssetReturnThresholdView(string Code, string LabelText, string JobCodeText, string Ceiling, string MaximumText, string StatusLabel)
{
    public static FallbackAssetReturnThresholdView ToFallbackAssetReturnThresholdView(FallbackAssetReturnThreshold item)
    {
        return new FallbackAssetReturnThresholdView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.JobCode,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAssetReturnThresholdView> ToFallbackAssetReturnThresholdViews(List<FallbackAssetReturnThreshold> items)
    {
        var views = new List<FallbackAssetReturnThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAssetReturnThresholdView(item));
        }
        return views;
    }

    public static double SumFallbackAssetReturnThresholdView(FallbackAssetReturnThresholdView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMaximum;
    }
}
