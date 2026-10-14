namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled deduction cycle is in its life cycle.</summary>
public enum ScheduledDeductionCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled deduction cycle, with what the planners need to act on it.</summary>
public sealed class ScheduledDeductionCycle(string code, string title, double threshold, double rate, string managerId, string unit, double minimum, double fte, string source, ScheduledDeductionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public double Rate { get; set; } = rate;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public string Source { get; set; } = source;
    public ScheduledDeductionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled deduction cycle in status proposed.</summary>
    public static ScheduledDeductionCycle NewScheduledDeductionCycle(string code, string title)
    {
        return new ScheduledDeductionCycle(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            ScheduledDeductionCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledDeductionCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledDeductionCycleStatus next)
    {
        switch (Status)
        {
            case ScheduledDeductionCycleStatus.Proposed:
                return next == ScheduledDeductionCycleStatus.Rejected;
            case ScheduledDeductionCycleStatus.Approved:
                return next == ScheduledDeductionCycleStatus.Rejected || next == ScheduledDeductionCycleStatus.Withdrawn;
            case ScheduledDeductionCycleStatus.Rejected:
                return next == ScheduledDeductionCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledDeductionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled deduction cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Threshold + Rate, 2);
    }

    public ScheduledDeductionCycle Copy()
    {
        return new ScheduledDeductionCycle(
            Code,
            Title,
            Threshold,
            Rate,
            ManagerId,
            Unit,
            Minimum,
            Fte,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
