namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched occupancy mapping.</summary>
public sealed record BatchedOccupancyMappingView(string Code, string Name, string Approved, string ExternalRefText, string Limit, string Reviewer, string Discount, string Active, string Rating, string StatusLabel)
{
    public static BatchedOccupancyMappingView ToBatchedOccupancyMappingView(BatchedOccupancyMapping item)
    {
        return new BatchedOccupancyMappingView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Approved}",
            Text.Truncate(item.ExternalRef, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedOccupancyMappingView> ToBatchedOccupancyMappingViews(List<BatchedOccupancyMapping> items)
    {
        var views = new List<BatchedOccupancyMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedOccupancyMappingView(item));
        }
        return views;
    }

    public static double SumBatchedOccupancyMappingView(BatchedOccupancyMappingView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedDiscount + parsedRating;
    }
}
