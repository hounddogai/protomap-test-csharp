namespace Acme.Workforce.Engagement.TownHall.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched town hall batch.</summary>
public sealed record BatchedTownHallBatchView(string Code, string Label, string Phone, string Weight, string BadgeText, string StatusLabel)
{
    public static BatchedTownHallBatchView ToBatchedTownHallBatchView(BatchedTownHallBatch item)
    {
        return new BatchedTownHallBatchView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Phone, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTownHallBatchView> ToBatchedTownHallBatchViews(List<BatchedTownHallBatch> items)
    {
        var views = new List<BatchedTownHallBatchView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTownHallBatchView(item));
        }
        return views;
    }

    public static double SumBatchedTownHallBatchView(BatchedTownHallBatchView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
