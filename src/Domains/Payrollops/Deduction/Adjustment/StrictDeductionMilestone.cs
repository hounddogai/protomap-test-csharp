namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict deduction milestone is in its life cycle.</summary>
public enum StrictDeductionMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict deduction milestone and its current status.</summary>
public sealed class StrictDeductionMilestone(string code, string name, int capacity, string managerId, string label, bool archived, int graceDays, StrictDeductionMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string ManagerId { get; set; } = managerId;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int GraceDays { get; set; } = graceDays;
    public StrictDeductionMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a strict deduction milestone in status proposed.</summary>
    public static StrictDeductionMilestone NewStrictDeductionMilestone(string code, string name)
    {
        return new StrictDeductionMilestone(
            code,
            name,
            0,
            "",
            "",
            false,
            0,
            StrictDeductionMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeductionMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeductionMilestoneStatus next)
    {
        switch (Status)
        {
            case StrictDeductionMilestoneStatus.Proposed:
                switch (next)
                {
                    case StrictDeductionMilestoneStatus.Rejected:
                    case StrictDeductionMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictDeductionMilestoneStatus.Approved:
                switch (next)
                {
                    case StrictDeductionMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictDeductionMilestoneStatus.Rejected:
                return next == StrictDeductionMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictDeductionMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict deduction milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Capacity + (double)GraceDays, 2);
    }

    public StrictDeductionMilestone Copy()
    {
        return new StrictDeductionMilestone(Code, Name, Capacity, ManagerId, Label, Archived, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
