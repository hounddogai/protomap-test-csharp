namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan batch.</summary>
public sealed record FloorPlanBatchView(string Code, string Name, string DiscountText, string Grade, string Checksum, string Phone, string Visible, string HoursText, string StatusLabel)
{
    public static FloorPlanBatchView ToFloorPlanBatchView(FloorPlanBatch item)
    {
        return new FloorPlanBatchView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Checksum,
            item.Phone,
            $"{item.Visible}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanBatchView> ToFloorPlanBatchViews(List<FloorPlanBatch> items)
    {
        var views = new List<FloorPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanBatchView(item));
        }
        return views;
    }

    public static double SumFloorPlanBatchView(FloorPlanBatchView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedHours;
    }
}
