namespace Acme.Workforce.Compensation.Benchmark.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched benchmark entry.</summary>
public sealed record BatchedBenchmarkEntryView(string Code, string Title, string EmployeeId, string Fte, string Ratio, string SequenceText, string CostCenter, string DurationMinutesText, string Score, string Reviewer, string StartsAt, string StatusLabel)
{
    public static BatchedBenchmarkEntryView ToBatchedBenchmarkEntryView(BatchedBenchmarkEntry item)
    {
        return new BatchedBenchmarkEntryView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.EmployeeId,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedBenchmarkEntryView> ToBatchedBenchmarkEntryViews(List<BatchedBenchmarkEntry> items)
    {
        var views = new List<BatchedBenchmarkEntryView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedBenchmarkEntryView(item));
        }
        return views;
    }

    public static double SumBatchedBenchmarkEntryView(BatchedBenchmarkEntryView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedFte + parsedRatio + (double)parsedSequence;
    }
}
