namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark tier is in its life cycle.</summary>
public enum BenchmarkTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A benchmark tier. Its code is unique within a tenant.</summary>
public sealed class BenchmarkTier(string code, string title, string ownerId, string summary, double score, int leaveDays, string description, int windowDays, BenchmarkTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public int LeaveDays { get; set; } = leaveDays;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public BenchmarkTierStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark tier in status open.</summary>
    public static BenchmarkTier NewBenchmarkTier(string code, string title)
    {
        return new BenchmarkTier(code, title, "", "", 0.0, 0, "", 0, BenchmarkTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkTierStatus next)
    {
        switch (Status)
        {
            case BenchmarkTierStatus.Open:
                return next == BenchmarkTierStatus.InReview || next == BenchmarkTierStatus.Reopened;
            case BenchmarkTierStatus.InReview:
                return next == BenchmarkTierStatus.Reopened || next == BenchmarkTierStatus.Archived;
            case BenchmarkTierStatus.Resolved:
                switch (next)
                {
                    case BenchmarkTierStatus.Reopened:
                    case BenchmarkTierStatus.Archived:
                        return true;
                }
                return false;
            case BenchmarkTierStatus.Reopened:
                return next == BenchmarkTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BenchmarkTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)LeaveDays + (double)WindowDays, 2);
    }

    public BenchmarkTier Copy()
    {
        return new BenchmarkTier(Code, Title, OwnerId, Summary, Score, LeaveDays, Description, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
