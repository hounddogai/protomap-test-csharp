namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction plan is in its life cycle.</summary>
public enum DeductionPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A deduction plan as the deduction team keeps it.</summary>
public sealed class DeductionPlan(string code, string label, bool pinned, double ceiling, string segment, int windowDays, int durationMinutes, string department, int startsAt, int leaveDays, DeductionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Ceiling { get; set; } = ceiling;
    public string Segment { get; set; } = segment;
    public int WindowDays { get; set; } = windowDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public DeductionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a deduction plan in status new.</summary>
    public static DeductionPlan NewDeductionPlan(string code, string label)
    {
        return new DeductionPlan(code, label, false, 0.0, "", 0, 0, "", 0, 0, DeductionPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionPlanStatus next)
    {
        switch (Status)
        {
            case DeductionPlanStatus.New:
                return next == DeductionPlanStatus.Failed;
            case DeductionPlanStatus.Queued:
                return next == DeductionPlanStatus.Running;
            case DeductionPlanStatus.Running:
                return next == DeductionPlanStatus.Done;
            case DeductionPlanStatus.Done:
                switch (next)
                {
                    case DeductionPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)DurationMinutes + (double)StartsAt, 2);
    }

    public DeductionPlan Copy()
    {
        return new DeductionPlan(
            Code,
            Label,
            Pinned,
            Ceiling,
            Segment,
            WindowDays,
            DurationMinutes,
            Department,
            StartsAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
