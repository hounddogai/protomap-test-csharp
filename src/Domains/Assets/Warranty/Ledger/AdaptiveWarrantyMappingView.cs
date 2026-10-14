namespace Acme.Workforce.Assets.Warranty.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive warranty mapping.</summary>
public sealed record AdaptiveWarrantyMappingView(string Code, string Label, string Quantity, string Ratio, string DurationMinutesText, string ExpiresAtText, string EndsAt, string LeaveDays, string Score, string StatusLabel)
{
    public static AdaptiveWarrantyMappingView ToAdaptiveWarrantyMappingView(AdaptiveWarrantyMapping item)
    {
        return new AdaptiveWarrantyMappingView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveWarrantyMappingView> ToAdaptiveWarrantyMappingViews(List<AdaptiveWarrantyMapping> items)
    {
        var views = new List<AdaptiveWarrantyMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveWarrantyMappingView(item));
        }
        return views;
    }

    public static double SumAdaptiveWarrantyMappingView(AdaptiveWarrantyMappingView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedRatio + (double)parsedDurationMinutes;
    }
}
