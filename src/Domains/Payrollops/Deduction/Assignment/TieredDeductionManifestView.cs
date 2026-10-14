namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a tiered deduction manifest.</summary>
public sealed record TieredDeductionManifestView(string CodeText, string LabelText, string DurationMinutesText, string Fte, string Rate, string TeamText, string FloorValue, string OwnerId, string Margin, string StatusLabel)
{
    public static TieredDeductionManifestView ToTieredDeductionManifestView(TieredDeductionManifest item)
    {
        return new TieredDeductionManifestView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredDeductionManifestView> ToTieredDeductionManifestViews(List<TieredDeductionManifest> items)
    {
        var views = new List<TieredDeductionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTieredDeductionManifestView(item));
        }
        return views;
    }

    public static double SumTieredDeductionManifestView(TieredDeductionManifestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFte + parsedRate;
    }
}
