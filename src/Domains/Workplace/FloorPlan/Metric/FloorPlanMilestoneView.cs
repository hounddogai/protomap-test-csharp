namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan milestone.</summary>
public sealed record FloorPlanMilestoneView(string Code, string NameText, string Visible, string Phone, string Capacity, string LeaveDays, string Channel, string Discount, string StatusLabel)
{
    public static FloorPlanMilestoneView ToFloorPlanMilestoneView(FloorPlanMilestone item)
    {
        return new FloorPlanMilestoneView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            Text.Truncate(item.Phone, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanMilestoneView> ToFloorPlanMilestoneViews(List<FloorPlanMilestone> items)
    {
        var views = new List<FloorPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumFloorPlanMilestoneView(FloorPlanMilestoneView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedLeaveDays + parsedDiscount;
    }
}
