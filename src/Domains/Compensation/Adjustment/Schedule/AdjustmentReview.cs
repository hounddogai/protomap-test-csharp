namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment review is in its life cycle.</summary>
public enum AdjustmentReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adjustment review, with what the planners need to act on it.</summary>
public sealed class AdjustmentReview(string code, string name, int attempts, string country, double cost, int priority, double discount, bool archived, AdjustmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public int Priority { get; set; } = priority;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public AdjustmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment review in status planned.</summary>
    public static AdjustmentReview NewAdjustmentReview(string code, string name)
    {
        return new AdjustmentReview(code, name, 0, "", 0.0, 0, 0.0, false, AdjustmentReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentReviewStatus next)
    {
        switch (Status)
        {
            case AdjustmentReviewStatus.Planned:
                switch (next)
                {
                    case AdjustmentReviewStatus.InProgress:
                    case AdjustmentReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentReviewStatus.Scheduled:
                switch (next)
                {
                    case AdjustmentReviewStatus.Completed:
                    case AdjustmentReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentReviewStatus.InProgress:
                return next == AdjustmentReviewStatus.Completed;
            case AdjustmentReviewStatus.Completed:
                return next == AdjustmentReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdjustmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Cost - (double)Attempts, 2);
    }

    public AdjustmentReview Copy()
    {
        return new AdjustmentReview(Code, Name, Attempts, Country, Cost, Priority, Discount, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
