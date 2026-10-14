namespace Acme.Workforce.Workplace.FloorPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled floor plan target.</summary>
public sealed record ScheduledFloorPlanTargetView(string Code, string LabelText, string Category, string CostCenter, string ExternalRef, string Team, string CreatedAt, string Channel, string Locale, string LeaveDays, string RateText, string StatusLabel)
{
    public static ScheduledFloorPlanTargetView ToScheduledFloorPlanTargetView(ScheduledFloorPlanTarget item)
    {
        return new ScheduledFloorPlanTargetView(
            Text.Truncate(item.Code, 40),
            item.Label,
            Text.Truncate(item.Category, 40),
            item.CostCenter,
            item.ExternalRef,
            item.Team,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Locale, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFloorPlanTargetView> ToScheduledFloorPlanTargetViews(List<ScheduledFloorPlanTarget> items)
    {
        var views = new List<ScheduledFloorPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFloorPlanTargetView(item));
        }
        return views;
    }

    public static double SumScheduledFloorPlanTargetView(ScheduledFloorPlanTargetView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLeaveDays + parsedRate;
    }
}
