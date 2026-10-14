namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling laptop review is in its life cycle.</summary>
public enum RollingLaptopReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rolling laptop review and its current status.</summary>
public sealed class RollingLaptopReview(string code, string label, string department, double ratio, int leaveDays, double maximum, string currency, string contactEmail, RollingLaptopReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public double Ratio { get; set; } = ratio;
    public int LeaveDays { get; set; } = leaveDays;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public RollingLaptopReviewStatus Status { get; set; } = status;

    /// <summary>Starts a rolling laptop review in status new.</summary>
    public static RollingLaptopReview NewRollingLaptopReview(string code, string label)
    {
        return new RollingLaptopReview(code, label, "", 0.0, 0, 0.0, "", "", RollingLaptopReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLaptopReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLaptopReviewStatus next)
    {
        switch (Status)
        {
            case RollingLaptopReviewStatus.New:
                switch (next)
                {
                    case RollingLaptopReviewStatus.Done:
                    case RollingLaptopReviewStatus.Failed:
                        return true;
                }
                return false;
            case RollingLaptopReviewStatus.Queued:
                switch (next)
                {
                    case RollingLaptopReviewStatus.Done:
                    case RollingLaptopReviewStatus.Failed:
                        return true;
                }
                return false;
            case RollingLaptopReviewStatus.Running:
                return next == RollingLaptopReviewStatus.Failed;
            case RollingLaptopReviewStatus.Done:
                switch (next)
                {
                    case RollingLaptopReviewStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLaptopReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling laptop review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Maximum * Ratio, 2);
    }

    public RollingLaptopReview Copy()
    {
        return new RollingLaptopReview(
            Code,
            Label,
            Department,
            Ratio,
            LeaveDays,
            Maximum,
            Currency,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
