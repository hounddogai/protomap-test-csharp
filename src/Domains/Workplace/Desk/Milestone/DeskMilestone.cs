namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk milestone is in its life cycle.</summary>
public enum DeskMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A desk milestone. Its code is unique within a tenant.</summary>
public sealed class DeskMilestone(string code, string name, int leaveDays, string label, int version, string country, int durationMinutes, DeskMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public DeskMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a desk milestone in status proposed.</summary>
    public static DeskMilestone NewDeskMilestone(string code, string name)
    {
        return new DeskMilestone(code, name, 0, "", 0, "", 0, DeskMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskMilestoneStatus next)
    {
        switch (Status)
        {
            case DeskMilestoneStatus.Proposed:
                switch (next)
                {
                    case DeskMilestoneStatus.Approved:
                        return true;
                }
                return false;
            case DeskMilestoneStatus.Approved:
                switch (next)
                {
                    case DeskMilestoneStatus.Rejected:
                        return true;
                }
                return false;
            case DeskMilestoneStatus.Rejected:
                return next == DeskMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeskMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"desk milestone"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays * (double)Version, 2);
    }

    public DeskMilestone Copy()
    {
        return new DeskMilestone(Code, Name, LeaveDays, Label, Version, Country, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
