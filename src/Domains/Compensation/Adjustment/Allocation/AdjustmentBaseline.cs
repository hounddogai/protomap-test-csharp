namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment baseline is in its life cycle.</summary>
public enum AdjustmentBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adjustment baseline as the adjustment team keeps it.</summary>
public sealed class AdjustmentBaseline(string code, string title, int graceDays, double threshold, double budget, AdjustmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public AdjustmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment baseline in status planned.</summary>
    public static AdjustmentBaseline NewAdjustmentBaseline(string code, string title)
    {
        return new AdjustmentBaseline(code, title, 0, 0.0, 0.0, AdjustmentBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentBaselineStatus next)
    {
        switch (Status)
        {
            case AdjustmentBaselineStatus.Planned:
                return next == AdjustmentBaselineStatus.Scheduled || next == AdjustmentBaselineStatus.InProgress;
            case AdjustmentBaselineStatus.Scheduled:
                return next == AdjustmentBaselineStatus.InProgress || next == AdjustmentBaselineStatus.Cancelled;
            case AdjustmentBaselineStatus.InProgress:
                return next == AdjustmentBaselineStatus.Completed;
            case AdjustmentBaselineStatus.Completed:
                return next == AdjustmentBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdjustmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Threshold * (double)GraceDays, 2);
    }

    public AdjustmentBaseline Copy()
    {
        return new AdjustmentBaseline(Code, Title, GraceDays, Threshold, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
