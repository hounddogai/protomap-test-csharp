namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using System.Globalization;

/// <summary>How screens and exports show a strict occupancy batch.</summary>
public sealed record StrictOccupancyBatchView(string Code, string TitleText, string PinnedText, string BatchSize, string Threshold, string CreatedAt, string Summary, string OwnerIdText, string StatusLabel)
{
    public static StrictOccupancyBatchView ToStrictOccupancyBatchView(StrictOccupancyBatch item)
    {
        return new StrictOccupancyBatchView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictOccupancyBatchView> ToStrictOccupancyBatchViews(List<StrictOccupancyBatch> items)
    {
        var views = new List<StrictOccupancyBatchView>();
        foreach (var item in items)
        {
            views.Add(ToStrictOccupancyBatchView(item));
        }
        return views;
    }

    public static double SumStrictOccupancyBatchView(StrictOccupancyBatchView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedThreshold + (double)parsedCreatedAt;
    }
}
