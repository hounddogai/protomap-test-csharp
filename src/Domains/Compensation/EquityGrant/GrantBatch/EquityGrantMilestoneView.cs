namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant milestone.</summary>
public sealed record EquityGrantMilestoneView(string CodeText, string Title, string QuantityText, string HoursText, string Capacity, string Grade, string StatusLabel)
{
    public static EquityGrantMilestoneView ToEquityGrantMilestoneView(EquityGrantMilestone item)
    {
        return new EquityGrantMilestoneView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantMilestoneView> ToEquityGrantMilestoneViews(List<EquityGrantMilestone> items)
    {
        var views = new List<EquityGrantMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantMilestoneView(item));
        }
        return views;
    }

    public static double SumEquityGrantMilestoneView(EquityGrantMilestoneView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedHours + (double)parsedCapacity;
    }
}
