namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict garnishment snapshot.</summary>
public sealed record StrictGarnishmentSnapshotView(string Code, string TitleText, string Retries, string Amount, string Threshold, string MinimumText, string DurationMinutes, string RegionText, string Badge, string Weight, string StatusLabel)
{
    public static StrictGarnishmentSnapshotView ToStrictGarnishmentSnapshotView(StrictGarnishmentSnapshot item)
    {
        return new StrictGarnishmentSnapshotView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Badge,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictGarnishmentSnapshotView> ToStrictGarnishmentSnapshotViews(List<StrictGarnishmentSnapshot> items)
    {
        var views = new List<StrictGarnishmentSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToStrictGarnishmentSnapshotView(item));
        }
        return views;
    }

    public static double SumStrictGarnishmentSnapshotView(StrictGarnishmentSnapshotView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount + parsedThreshold;
    }
}
