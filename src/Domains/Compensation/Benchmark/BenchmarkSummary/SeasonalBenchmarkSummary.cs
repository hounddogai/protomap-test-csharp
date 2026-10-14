namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal benchmark summary is in its life cycle.</summary>
public enum SeasonalBenchmarkSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a seasonal benchmark summary and its current status.</summary>
public sealed class SeasonalBenchmarkSummary(string code, string name, string salaryBand, string notes, int priority, SeasonalBenchmarkSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public SeasonalBenchmarkSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal benchmark summary in status planned.</summary>
    public static SeasonalBenchmarkSummary NewSeasonalBenchmarkSummary(string code, string name)
    {
        return new SeasonalBenchmarkSummary(code, name, "", "", 0, SeasonalBenchmarkSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBenchmarkSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBenchmarkSummaryStatus next)
    {
        switch (Status)
        {
            case SeasonalBenchmarkSummaryStatus.Planned:
                return next == SeasonalBenchmarkSummaryStatus.Completed || next == SeasonalBenchmarkSummaryStatus.Cancelled;
            case SeasonalBenchmarkSummaryStatus.Scheduled:
                switch (next)
                {
                    case SeasonalBenchmarkSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalBenchmarkSummaryStatus.InProgress:
                switch (next)
                {
                    case SeasonalBenchmarkSummaryStatus.Completed:
                    case SeasonalBenchmarkSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalBenchmarkSummaryStatus.Completed:
                switch (next)
                {
                    case SeasonalBenchmarkSummaryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBenchmarkSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal benchmark summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal benchmark summary"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public SeasonalBenchmarkSummary Copy()
    {
        return new SeasonalBenchmarkSummary(Code, Name, SalaryBand, Notes, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
