namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary training due plan is in its life cycle.</summary>
public enum PrimaryTrainingDuePlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary training due plan as the training due team keeps it.</summary>
public sealed class PrimaryTrainingDuePlan(string code, string title, string timezone, int windowDays, int tenureMonths, string badge, double threshold, string checksum, int priority, PrimaryTrainingDuePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public double Threshold { get; set; } = threshold;
    public string Checksum { get; set; } = checksum;
    public int Priority { get; set; } = priority;
    public PrimaryTrainingDuePlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary training due plan in status open.</summary>
    public static PrimaryTrainingDuePlan NewPrimaryTrainingDuePlan(string code, string title)
    {
        return new PrimaryTrainingDuePlan(code, title, "", 0, 0, "", 0.0, "", 0, PrimaryTrainingDuePlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTrainingDuePlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTrainingDuePlanStatus next)
    {
        switch (Status)
        {
            case PrimaryTrainingDuePlanStatus.Open:
                return next == PrimaryTrainingDuePlanStatus.InReview;
            case PrimaryTrainingDuePlanStatus.InReview:
                switch (next)
                {
                    case PrimaryTrainingDuePlanStatus.Resolved:
                    case PrimaryTrainingDuePlanStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryTrainingDuePlanStatus.Resolved:
                switch (next)
                {
                    case PrimaryTrainingDuePlanStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryTrainingDuePlanStatus.Reopened:
                switch (next)
                {
                    case PrimaryTrainingDuePlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryTrainingDuePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary training due plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Threshold + (double)Priority, 2);
    }

    public PrimaryTrainingDuePlan Copy()
    {
        return new PrimaryTrainingDuePlan(
            Code,
            Title,
            Timezone,
            WindowDays,
            TenureMonths,
            Badge,
            Threshold,
            Checksum,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
