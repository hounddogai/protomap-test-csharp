namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled adjustment allocation.</summary>
public sealed record ScheduledAdjustmentAllocationView(string Code, string Title, string Badge, string WeightText, string Timezone, string Region, string ManagerIdText, string LocationText, string Visible, string StatusLabel)
{
    public static ScheduledAdjustmentAllocationView ToScheduledAdjustmentAllocationView(ScheduledAdjustmentAllocation item)
    {
        return new ScheduledAdjustmentAllocationView(
            item.Code,
            item.Title,
            Text.Truncate(item.Badge, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Region,
            item.ManagerId,
            item.Location,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAdjustmentAllocationView> ToScheduledAdjustmentAllocationViews(List<ScheduledAdjustmentAllocation> items)
    {
        var views = new List<ScheduledAdjustmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAdjustmentAllocationView(item));
        }
        return views;
    }

    public static double SumScheduledAdjustmentAllocationView(ScheduledAdjustmentAllocationView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
