namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional timesheet milestone is in its life cycle.</summary>
public enum RegionalTimesheetMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional timesheet milestone. Its code is unique within a tenant.</summary>
public sealed class RegionalTimesheetMilestone(string code, string name, int capacity, double score, double cost, double ceiling, string costCenter, RegionalTimesheetMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public RegionalTimesheetMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a regional timesheet milestone in status open.</summary>
    public static RegionalTimesheetMilestone NewRegionalTimesheetMilestone(string code, string name)
    {
        return new RegionalTimesheetMilestone(
            code,
            name,
            0,
            0.0,
            0.0,
            0.0,
            "",
            RegionalTimesheetMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTimesheetMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTimesheetMilestoneStatus next)
    {
        switch (Status)
        {
            case RegionalTimesheetMilestoneStatus.Open:
                return next == RegionalTimesheetMilestoneStatus.InReview || next == RegionalTimesheetMilestoneStatus.Resolved;
            case RegionalTimesheetMilestoneStatus.InReview:
                return next == RegionalTimesheetMilestoneStatus.Archived;
            case RegionalTimesheetMilestoneStatus.Resolved:
                return next == RegionalTimesheetMilestoneStatus.Reopened;
            case RegionalTimesheetMilestoneStatus.Reopened:
                switch (next)
                {
                    case RegionalTimesheetMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalTimesheetMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional timesheet milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score - Ceiling, 2);
    }

    public RegionalTimesheetMilestone Copy()
    {
        return new RegionalTimesheetMilestone(Code, Name, Capacity, Score, Cost, Ceiling, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
