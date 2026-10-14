namespace Acme.Workforce.Workplace.Occupancy.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted occupancy batch.</summary>
public sealed record WeightedOccupancyBatchView(string Code, string NameText, string Ceiling, string Discount, string FloorValue, string Ratio, string Priority, string UpdatedAtText, string Timezone, string Category, string StatusLabel)
{
    public static WeightedOccupancyBatchView ToWeightedOccupancyBatchView(WeightedOccupancyBatch item)
    {
        return new WeightedOccupancyBatchView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.Category, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedOccupancyBatchView> ToWeightedOccupancyBatchViews(List<WeightedOccupancyBatch> items)
    {
        var views = new List<WeightedOccupancyBatchView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedOccupancyBatchView(item));
        }
        return views;
    }

    public static double SumWeightedOccupancyBatchView(WeightedOccupancyBatchView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedDiscount + parsedFloorValue;
    }
}
