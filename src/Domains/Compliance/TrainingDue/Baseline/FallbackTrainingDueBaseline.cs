namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback training due baseline is in its life cycle.</summary>
public enum FallbackTrainingDueBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback training due baseline as the training due team keeps it.</summary>
public sealed class FallbackTrainingDueBaseline(string code, string name, double ceiling, string badge, string title, string checksum, double hours, double score, FallbackTrainingDueBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Hours { get; set; } = hours;
    public double Score { get; set; } = score;
    public FallbackTrainingDueBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a fallback training due baseline in status planned.</summary>
    public static FallbackTrainingDueBaseline NewFallbackTrainingDueBaseline(string code, string name)
    {
        return new FallbackTrainingDueBaseline(
            code,
            name,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            FallbackTrainingDueBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTrainingDueBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTrainingDueBaselineStatus next)
    {
        switch (Status)
        {
            case FallbackTrainingDueBaselineStatus.Planned:
                switch (next)
                {
                    case FallbackTrainingDueBaselineStatus.Scheduled:
                    case FallbackTrainingDueBaselineStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackTrainingDueBaselineStatus.Scheduled:
                switch (next)
                {
                    case FallbackTrainingDueBaselineStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackTrainingDueBaselineStatus.InProgress:
                return next == FallbackTrainingDueBaselineStatus.Completed || next == FallbackTrainingDueBaselineStatus.Cancelled;
            case FallbackTrainingDueBaselineStatus.Completed:
                return next == FallbackTrainingDueBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackTrainingDueBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback training due baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Ceiling - Score, 2);
    }

    public FallbackTrainingDueBaseline Copy()
    {
        return new FallbackTrainingDueBaseline(Code, Name, Ceiling, Badge, Title, Checksum, Hours, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
