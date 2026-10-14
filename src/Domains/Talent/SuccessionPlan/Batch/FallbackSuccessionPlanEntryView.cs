namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback succession plan entry.</summary>
public sealed record FallbackSuccessionPlanEntryView(string Code, string Label, string Locale, string DurationMinutes, string Notes, string EmployeeId, string BatchSizeText, string FloorValue, string StatusLabel)
{
    public static FallbackSuccessionPlanEntryView ToFallbackSuccessionPlanEntryView(FallbackSuccessionPlanEntry item)
    {
        return new FallbackSuccessionPlanEntryView(
            item.Code,
            item.Label,
            item.Locale,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.EmployeeId, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSuccessionPlanEntryView> ToFallbackSuccessionPlanEntryViews(List<FallbackSuccessionPlanEntry> items)
    {
        var views = new List<FallbackSuccessionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSuccessionPlanEntryView(item));
        }
        return views;
    }

    public static double SumFallbackSuccessionPlanEntryView(FallbackSuccessionPlanEntryView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedBatchSize + parsedFloorValue;
    }
}
