namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling benchmark summary is in its life cycle.</summary>
public enum RollingBenchmarkSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling benchmark summary, with what the planners need to act on it.</summary>
public sealed class RollingBenchmarkSummary(string code, string title, string managerId, int retries, string region, string skill, double ceiling, bool pinned, string jobCode, string description, RollingBenchmarkSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public bool Pinned { get; set; } = pinned;
    public string JobCode { get; set; } = jobCode;
    public string Description { get; set; } = description;
    public RollingBenchmarkSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling benchmark summary in status new.</summary>
    public static RollingBenchmarkSummary NewRollingBenchmarkSummary(string code, string title)
    {
        return new RollingBenchmarkSummary(
            code,
            title,
            "",
            0,
            "",
            "",
            0.0,
            false,
            "",
            "",
            RollingBenchmarkSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBenchmarkSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBenchmarkSummaryStatus next)
    {
        switch (Status)
        {
            case RollingBenchmarkSummaryStatus.New:
                switch (next)
                {
                    case RollingBenchmarkSummaryStatus.Running:
                    case RollingBenchmarkSummaryStatus.Failed:
                        return true;
                }
                return false;
            case RollingBenchmarkSummaryStatus.Queued:
                switch (next)
                {
                    case RollingBenchmarkSummaryStatus.Done:
                        return true;
                }
                return false;
            case RollingBenchmarkSummaryStatus.Running:
                return next == RollingBenchmarkSummaryStatus.Failed;
            case RollingBenchmarkSummaryStatus.Done:
                return next == RollingBenchmarkSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingBenchmarkSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling benchmark summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling benchmark summary"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Ceiling * (double)Retries, 2);
    }

    public RollingBenchmarkSummary Copy()
    {
        return new RollingBenchmarkSummary(
            Code,
            Title,
            ManagerId,
            Retries,
            Region,
            Skill,
            Ceiling,
            Pinned,
            JobCode,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
