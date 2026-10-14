namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark summary is in its life cycle.</summary>
public enum BenchmarkSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark summary. Its code is unique within a tenant.</summary>
public sealed class BenchmarkSummary(string code, string label, bool pinned, double hours, double budget, BenchmarkSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public BenchmarkSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark summary in status new.</summary>
    public static BenchmarkSummary NewBenchmarkSummary(string code, string label)
    {
        return new BenchmarkSummary(code, label, false, 0.0, 0.0, BenchmarkSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkSummaryStatus next)
    {
        switch (Status)
        {
            case BenchmarkSummaryStatus.New:
                switch (next)
                {
                    case BenchmarkSummaryStatus.Done:
                        return true;
                }
                return false;
            case BenchmarkSummaryStatus.Queued:
                return next == BenchmarkSummaryStatus.Done;
            case BenchmarkSummaryStatus.Running:
                return next == BenchmarkSummaryStatus.Failed;
            case BenchmarkSummaryStatus.Done:
                switch (next)
                {
                    case BenchmarkSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Hours - Budget, 2);
    }

    public BenchmarkSummary Copy()
    {
        return new BenchmarkSummary(Code, Label, Pinned, Hours, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
