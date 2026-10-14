namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan exception is in its life cycle.</summary>
public enum ActionPlanExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A action plan exception as the action plan team keeps it.</summary>
public sealed class ActionPlanException(string code, string title, double threshold, string segment, string unit, string reviewer, List<string> tags, string skill, double margin, string employeeId, ActionPlanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Segment { get; set; } = segment;
    public string Unit { get; set; } = unit;
    public string Reviewer { get; set; } = reviewer;
    public List<string> Tags { get; set; } = tags;
    public string Skill { get; set; } = skill;
    public double Margin { get; set; } = margin;
    public string EmployeeId { get; set; } = employeeId;
    public ActionPlanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a action plan exception in status proposed.</summary>
    public static ActionPlanException NewActionPlanException(string code, string title)
    {
        return new ActionPlanException(
            code,
            title,
            0.0,
            "",
            "",
            "",
            new List<string>(),
            "",
            0.0,
            "",
            ActionPlanExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanExceptionStatus next)
    {
        switch (Status)
        {
            case ActionPlanExceptionStatus.Proposed:
                return next == ActionPlanExceptionStatus.Rejected;
            case ActionPlanExceptionStatus.Approved:
                switch (next)
                {
                    case ActionPlanExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case ActionPlanExceptionStatus.Rejected:
                switch (next)
                {
                    case ActionPlanExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round(Margin + Threshold, 2);
    }

    public ActionPlanException Copy()
    {
        return new ActionPlanException(
            Code,
            Title,
            Threshold,
            Segment,
            Unit,
            Reviewer,
            Tags,
            Skill,
            Margin,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
