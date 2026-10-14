namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan group is in its life cycle.</summary>
public enum ActionPlanGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A action plan group as the action plan team keeps it.</summary>
public sealed class ActionPlanGroup(string code, string label, double floorValue, string team, string summary, double rating, string category, int leaveDays, ActionPlanGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public int LeaveDays { get; set; } = leaveDays;
    public ActionPlanGroupStatus Status { get; set; } = status;

    /// <summary>Starts a action plan group in status planned.</summary>
    public static ActionPlanGroup NewActionPlanGroup(string code, string label)
    {
        return new ActionPlanGroup(code, label, 0.0, "", "", 0.0, "", 0, ActionPlanGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanGroupStatus next)
    {
        switch (Status)
        {
            case ActionPlanGroupStatus.Planned:
                return next == ActionPlanGroupStatus.InProgress;
            case ActionPlanGroupStatus.Scheduled:
                return next == ActionPlanGroupStatus.InProgress || next == ActionPlanGroupStatus.Cancelled;
            case ActionPlanGroupStatus.InProgress:
                switch (next)
                {
                    case ActionPlanGroupStatus.Completed:
                    case ActionPlanGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanGroupStatus.Completed:
                switch (next)
                {
                    case ActionPlanGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round(FloorValue + (double)LeaveDays, 2);
    }

    public ActionPlanGroup Copy()
    {
        return new ActionPlanGroup(Code, Label, FloorValue, Team, Summary, Rating, Category, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
