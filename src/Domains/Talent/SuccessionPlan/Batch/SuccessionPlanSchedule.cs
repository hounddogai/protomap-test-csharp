namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan schedule is in its life cycle.</summary>
public enum SuccessionPlanScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One succession plan schedule, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanSchedule(string code, string label, bool approved, int sequence, double margin, SuccessionPlanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int Sequence { get; set; } = sequence;
    public double Margin { get; set; } = margin;
    public SuccessionPlanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan schedule in status proposed.</summary>
    public static SuccessionPlanSchedule NewSuccessionPlanSchedule(string code, string label)
    {
        return new SuccessionPlanSchedule(code, label, false, 0, 0.0, SuccessionPlanScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanScheduleStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanScheduleStatus.Proposed:
                return next == SuccessionPlanScheduleStatus.Withdrawn;
            case SuccessionPlanScheduleStatus.Approved:
                return next == SuccessionPlanScheduleStatus.Rejected || next == SuccessionPlanScheduleStatus.Withdrawn;
            case SuccessionPlanScheduleStatus.Rejected:
                return next == SuccessionPlanScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Sequence - Margin, 2);
    }

    public SuccessionPlanSchedule Copy()
    {
        return new SuccessionPlanSchedule(Code, Label, Approved, Sequence, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
