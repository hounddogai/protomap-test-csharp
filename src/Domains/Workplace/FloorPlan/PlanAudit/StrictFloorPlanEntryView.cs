namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict floor plan entry.</summary>
public sealed record StrictFloorPlanEntryView(string CodeText, string Name, string Threshold, string ExternalRefText, string LeaveDays, string ExpiresAt, string Hours, string StatusLabel)
{
    public static StrictFloorPlanEntryView ToStrictFloorPlanEntryView(StrictFloorPlanEntry item)
    {
        return new StrictFloorPlanEntryView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictFloorPlanEntryView> ToStrictFloorPlanEntryViews(List<StrictFloorPlanEntry> items)
    {
        var views = new List<StrictFloorPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToStrictFloorPlanEntryView(item));
        }
        return views;
    }

    public static double SumStrictFloorPlanEntryView(StrictFloorPlanEntryView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLeaveDays + (double)parsedExpiresAt;
    }
}
