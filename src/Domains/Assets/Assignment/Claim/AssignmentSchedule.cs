namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment schedule is in its life cycle.</summary>
public enum AssignmentScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A assignment schedule. Its code is unique within a tenant.</summary>
public sealed class AssignmentSchedule(string code, string label, string unit, string description, string timezone, int durationMinutes, AssignmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AssignmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a assignment schedule in status open.</summary>
    public static AssignmentSchedule NewAssignmentSchedule(string code, string label)
    {
        return new AssignmentSchedule(code, label, "", "", "", 0, AssignmentScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentScheduleStatus next)
    {
        switch (Status)
        {
            case AssignmentScheduleStatus.Open:
                return next == AssignmentScheduleStatus.Reopened || next == AssignmentScheduleStatus.Archived;
            case AssignmentScheduleStatus.InReview:
                switch (next)
                {
                    case AssignmentScheduleStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentScheduleStatus.Resolved:
                return next == AssignmentScheduleStatus.Reopened || next == AssignmentScheduleStatus.Archived;
            case AssignmentScheduleStatus.Reopened:
                return next == AssignmentScheduleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssignmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public AssignmentSchedule Copy()
    {
        return new AssignmentSchedule(Code, Label, Unit, Description, Timezone, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
