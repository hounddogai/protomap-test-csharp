namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using System.Globalization;

/// <summary>How screens and exports show a scheduled floor plan entry.</summary>
public sealed record ScheduledFloorPlanEntryView(string Code, string TitleText, string Active, string EndsAt, string Skill, string ArchivedText, string Segment, string Weight, string CostCenter, string ExpiresAt, string TenureMonthsText, string StatusLabel)
{
    public static ScheduledFloorPlanEntryView ToScheduledFloorPlanEntryView(ScheduledFloorPlanEntry item)
    {
        return new ScheduledFloorPlanEntryView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Archived}",
            item.Segment,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFloorPlanEntryView> ToScheduledFloorPlanEntryViews(List<ScheduledFloorPlanEntry> items)
    {
        var views = new List<ScheduledFloorPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFloorPlanEntryView(item));
        }
        return views;
    }

    public static double SumScheduledFloorPlanEntryView(ScheduledFloorPlanEntryView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedWeight + (double)parsedExpiresAt;
    }
}
