namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional benchmark review is in its life cycle.</summary>
public enum RegionalBenchmarkReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One regional benchmark review, with what the planners need to act on it.</summary>
public sealed class RegionalBenchmarkReview(string code, string name, double minimum, double rate, string costCenter, double weight, int priority, double threshold, bool active, string phone, RegionalBenchmarkReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public double Threshold { get; set; } = threshold;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public RegionalBenchmarkReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional benchmark review in status open.</summary>
    public static RegionalBenchmarkReview NewRegionalBenchmarkReview(string code, string name)
    {
        return new RegionalBenchmarkReview(
            code,
            name,
            0.0,
            0.0,
            "",
            0.0,
            0,
            0.0,
            false,
            "",
            RegionalBenchmarkReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBenchmarkReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBenchmarkReviewStatus next)
    {
        switch (Status)
        {
            case RegionalBenchmarkReviewStatus.Open:
                return next == RegionalBenchmarkReviewStatus.Resolved;
            case RegionalBenchmarkReviewStatus.InReview:
                return next == RegionalBenchmarkReviewStatus.Resolved || next == RegionalBenchmarkReviewStatus.Reopened;
            case RegionalBenchmarkReviewStatus.Resolved:
                switch (next)
                {
                    case RegionalBenchmarkReviewStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalBenchmarkReviewStatus.Reopened:
                switch (next)
                {
                    case RegionalBenchmarkReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalBenchmarkReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional benchmark review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Threshold + (double)Priority, 2);
    }

    public RegionalBenchmarkReview Copy()
    {
        return new RegionalBenchmarkReview(
            Code,
            Name,
            Minimum,
            Rate,
            CostCenter,
            Weight,
            Priority,
            Threshold,
            Active,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
