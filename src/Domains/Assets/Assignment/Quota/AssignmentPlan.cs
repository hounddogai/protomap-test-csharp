namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment plan is in its life cycle.</summary>
public enum AssignmentPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One assignment plan, with what the planners need to act on it.</summary>
public sealed class AssignmentPlan(string code, string name, string summary, string region, string badge, string team, bool active, double score, AssignmentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public AssignmentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a assignment plan in status open.</summary>
    public static AssignmentPlan NewAssignmentPlan(string code, string name)
    {
        return new AssignmentPlan(code, name, "", "", "", "", false, 0.0, AssignmentPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentPlanStatus next)
    {
        switch (Status)
        {
            case AssignmentPlanStatus.Open:
                switch (next)
                {
                    case AssignmentPlanStatus.InReview:
                    case AssignmentPlanStatus.Resolved:
                        return true;
                }
                return false;
            case AssignmentPlanStatus.InReview:
                return next == AssignmentPlanStatus.Archived;
            case AssignmentPlanStatus.Resolved:
                return next == AssignmentPlanStatus.Archived;
            case AssignmentPlanStatus.Reopened:
                switch (next)
                {
                    case AssignmentPlanStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"assignment plan"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AssignmentPlan Copy()
    {
        return new AssignmentPlan(Code, Name, Summary, Region, Badge, Team, Active, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
