namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient adjustment schedule is in its life cycle.</summary>
public enum LenientAdjustmentScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient adjustment schedule. Its code is unique within a tenant.</summary>
public sealed class LenientAdjustmentSchedule(string code, string name, bool archived, double rating, int priority, string grade, LenientAdjustmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Rating { get; set; } = rating;
    public int Priority { get; set; } = priority;
    public string Grade { get; set; } = grade;
    public LenientAdjustmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient adjustment schedule in status planned.</summary>
    public static LenientAdjustmentSchedule NewLenientAdjustmentSchedule(string code, string name)
    {
        return new LenientAdjustmentSchedule(code, name, false, 0.0, 0, "", LenientAdjustmentScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAdjustmentScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAdjustmentScheduleStatus next)
    {
        switch (Status)
        {
            case LenientAdjustmentScheduleStatus.Planned:
                return next == LenientAdjustmentScheduleStatus.Scheduled;
            case LenientAdjustmentScheduleStatus.Scheduled:
                switch (next)
                {
                    case LenientAdjustmentScheduleStatus.InProgress:
                        return true;
                }
                return false;
            case LenientAdjustmentScheduleStatus.InProgress:
                return next == LenientAdjustmentScheduleStatus.Completed || next == LenientAdjustmentScheduleStatus.Cancelled;
            case LenientAdjustmentScheduleStatus.Completed:
                switch (next)
                {
                    case LenientAdjustmentScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAdjustmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient adjustment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating + (double)Priority, 2);
    }

    public LenientAdjustmentSchedule Copy()
    {
        return new LenientAdjustmentSchedule(Code, Name, Archived, Rating, Priority, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
