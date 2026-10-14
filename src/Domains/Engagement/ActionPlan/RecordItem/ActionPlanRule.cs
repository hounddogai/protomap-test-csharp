namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan rule is in its life cycle.</summary>
public enum ActionPlanRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a action plan rule and its current status.</summary>
public sealed class ActionPlanRule(string code, string name, string managerId, string employeeId, double rating, string grade, bool visible, int priority, string reviewer, string unit, ActionPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public string EmployeeId { get; set; } = employeeId;
    public double Rating { get; set; } = rating;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public string Unit { get; set; } = unit;
    public ActionPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a action plan rule in status created.</summary>
    public static ActionPlanRule NewActionPlanRule(string code, string name)
    {
        return new ActionPlanRule(code, name, "", "", 0.0, "", false, 0, "", "", ActionPlanRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanRuleStatus next)
    {
        switch (Status)
        {
            case ActionPlanRuleStatus.Created:
                return next == ActionPlanRuleStatus.Retired;
            case ActionPlanRuleStatus.Validated:
                switch (next)
                {
                    case ActionPlanRuleStatus.Published:
                        return true;
                }
                return false;
            case ActionPlanRuleStatus.Published:
                switch (next)
                {
                    case ActionPlanRuleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"action plan rule"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rating - (double)Priority, 2);
    }

    public ActionPlanRule Copy()
    {
        return new ActionPlanRule(
            Code,
            Name,
            ManagerId,
            EmployeeId,
            Rating,
            Grade,
            Visible,
            Priority,
            Reviewer,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
