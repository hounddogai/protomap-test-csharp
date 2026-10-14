namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched access badge target.</summary>
public sealed record BatchedAccessBadgeTargetView(string Code, string TitleText, string Hours, string Country, string ArchivedText, string CapacityText, string StatusLabel)
{
    public static BatchedAccessBadgeTargetView ToBatchedAccessBadgeTargetView(BatchedAccessBadgeTarget item)
    {
        return new BatchedAccessBadgeTargetView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Archived}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAccessBadgeTargetView> ToBatchedAccessBadgeTargetViews(List<BatchedAccessBadgeTarget> items)
    {
        var views = new List<BatchedAccessBadgeTargetView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAccessBadgeTargetView(item));
        }
        return views;
    }

    public static double SumBatchedAccessBadgeTargetView(BatchedAccessBadgeTargetView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedCapacity;
    }
}
