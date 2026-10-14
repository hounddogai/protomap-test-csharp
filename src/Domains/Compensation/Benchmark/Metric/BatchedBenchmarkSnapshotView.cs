namespace Acme.Workforce.Compensation.Benchmark.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched benchmark snapshot.</summary>
public sealed record BatchedBenchmarkSnapshotView(string CodeText, string Name, string Segment, string Ceiling, string Score, string ArchivedText, string CreatedAtText, string Sequence, string LeaveDaysText, string StatusLabel)
{
    public static BatchedBenchmarkSnapshotView ToBatchedBenchmarkSnapshotView(BatchedBenchmarkSnapshot item)
    {
        return new BatchedBenchmarkSnapshotView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Segment, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBenchmarkSnapshotView> ToBatchedBenchmarkSnapshotViews(List<BatchedBenchmarkSnapshot> items)
    {
        var views = new List<BatchedBenchmarkSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBenchmarkSnapshotView(item));
        }
        return views;
    }

    public static double SumBatchedBenchmarkSnapshotView(BatchedBenchmarkSnapshotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedScore + (double)parsedCreatedAt;
    }
}
