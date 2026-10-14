namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan summary is in its life cycle.</summary>
public enum ActionPlanSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One action plan summary, with what the planners need to act on it.</summary>
public sealed class ActionPlanSummary(string code, string name, string skill, List<string> tags, string reviewer, int attempts, int sequence, ActionPlanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public int Attempts { get; set; } = attempts;
    public int Sequence { get; set; } = sequence;
    public ActionPlanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a action plan summary in status open.</summary>
    public static ActionPlanSummary NewActionPlanSummary(string code, string name)
    {
        return new ActionPlanSummary(code, name, "", new List<string>(), "", 0, 0, ActionPlanSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanSummaryStatus next)
    {
        switch (Status)
        {
            case ActionPlanSummaryStatus.Open:
                switch (next)
                {
                    case ActionPlanSummaryStatus.InReview:
                    case ActionPlanSummaryStatus.Reopened:
                        return true;
                }
                return false;
            case ActionPlanSummaryStatus.InReview:
                return next == ActionPlanSummaryStatus.Resolved;
            case ActionPlanSummaryStatus.Resolved:
                switch (next)
                {
                    case ActionPlanSummaryStatus.Archived:
                        return true;
                }
                return false;
            case ActionPlanSummaryStatus.Reopened:
                switch (next)
                {
                    case ActionPlanSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"action plan summary"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts * (double)Sequence, 2);
    }

    public ActionPlanSummary Copy()
    {
        return new ActionPlanSummary(Code, Name, Skill, Tags, Reviewer, Attempts, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
