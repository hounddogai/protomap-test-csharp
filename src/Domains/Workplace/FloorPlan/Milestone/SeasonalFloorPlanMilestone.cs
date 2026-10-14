namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan milestone is in its life cycle.</summary>
public enum SeasonalFloorPlanMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal floor plan milestone and its current status.</summary>
public sealed class SeasonalFloorPlanMilestone(string code, string label, string source, string employeeId, string contactEmail, double score, SeasonalFloorPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public double Score { get; set; } = score;
    public SeasonalFloorPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan milestone in status draft.</summary>
    public static SeasonalFloorPlanMilestone NewSeasonalFloorPlanMilestone(string code, string label)
    {
        return new SeasonalFloorPlanMilestone(code, label, "", "", "", 0.0, SeasonalFloorPlanMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanMilestoneStatus.Draft:
                switch (next)
                {
                    case SeasonalFloorPlanMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalFloorPlanMilestoneStatus.Pending:
                switch (next)
                {
                    case SeasonalFloorPlanMilestoneStatus.Suspended:
                    case SeasonalFloorPlanMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalFloorPlanMilestoneStatus.Active:
                return next == SeasonalFloorPlanMilestoneStatus.Suspended || next == SeasonalFloorPlanMilestoneStatus.Closed;
            case SeasonalFloorPlanMilestoneStatus.Suspended:
                switch (next)
                {
                    case SeasonalFloorPlanMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public SeasonalFloorPlanMilestone Copy()
    {
        return new SeasonalFloorPlanMilestone(Code, Label, Source, EmployeeId, ContactEmail, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
