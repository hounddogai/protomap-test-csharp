namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan rollup is in its life cycle.</summary>
public enum ActionPlanRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One action plan rollup, with what the planners need to act on it.</summary>
public sealed class ActionPlanRollup(string code, string title, int retries, string skill, double ceiling, string category, string salaryBand, string country, double weight, ActionPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Skill { get; set; } = skill;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public string SalaryBand { get; set; } = salaryBand;
    public string Country { get; set; } = country;
    public double Weight { get; set; } = weight;
    public ActionPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a action plan rollup in status idle.</summary>
    public static ActionPlanRollup NewActionPlanRollup(string code, string title)
    {
        return new ActionPlanRollup(code, title, 0, "", 0.0, "", "", "", 0.0, ActionPlanRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanRollupStatus next)
    {
        switch (Status)
        {
            case ActionPlanRollupStatus.Idle:
                switch (next)
                {
                    case ActionPlanRollupStatus.Warming:
                    case ActionPlanRollupStatus.Draining:
                        return true;
                }
                return false;
            case ActionPlanRollupStatus.Warming:
                return next == ActionPlanRollupStatus.Ready || next == ActionPlanRollupStatus.Draining;
            case ActionPlanRollupStatus.Ready:
                switch (next)
                {
                    case ActionPlanRollupStatus.Draining:
                    case ActionPlanRollupStatus.Stopped:
                        return true;
                }
                return false;
            case ActionPlanRollupStatus.Draining:
                switch (next)
                {
                    case ActionPlanRollupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Retries * Weight, 2);
    }

    public ActionPlanRollup Copy()
    {
        return new ActionPlanRollup(
            Code,
            Title,
            Retries,
            Skill,
            Ceiling,
            Category,
            SalaryBand,
            Country,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
