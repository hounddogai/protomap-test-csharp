namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant review is in its life cycle.</summary>
public enum EquityGrantReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A equity grant review as the equity grant team keeps it.</summary>
public sealed class EquityGrantReview(string code, string name, double budget, double minimum, string reviewer, string timezone, double ceiling, int batchSize, double hours, bool active, EquityGrantReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public string Reviewer { get; set; } = reviewer;
    public string Timezone { get; set; } = timezone;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public EquityGrantReviewStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant review in status planned.</summary>
    public static EquityGrantReview NewEquityGrantReview(string code, string name)
    {
        return new EquityGrantReview(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0,
            0.0,
            false,
            EquityGrantReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantReviewStatus next)
    {
        switch (Status)
        {
            case EquityGrantReviewStatus.Planned:
                return next == EquityGrantReviewStatus.InProgress;
            case EquityGrantReviewStatus.Scheduled:
                switch (next)
                {
                    case EquityGrantReviewStatus.Completed:
                        return true;
                }
                return false;
            case EquityGrantReviewStatus.InProgress:
                return next == EquityGrantReviewStatus.Completed || next == EquityGrantReviewStatus.Cancelled;
            case EquityGrantReviewStatus.Completed:
                return next == EquityGrantReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EquityGrantReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"equity grant review"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Minimum * Hours, 2);
    }

    public EquityGrantReview Copy()
    {
        return new EquityGrantReview(
            Code,
            Name,
            Budget,
            Minimum,
            Reviewer,
            Timezone,
            Ceiling,
            BatchSize,
            Hours,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
