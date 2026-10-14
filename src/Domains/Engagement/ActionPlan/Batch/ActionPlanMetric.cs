namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan metric is in its life cycle.</summary>
public enum ActionPlanMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A action plan metric. Its code is unique within a tenant.</summary>
public sealed class ActionPlanMetric(string code, string title, int revision, string skill, string jobCode, ActionPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Skill { get; set; } = skill;
    public string JobCode { get; set; } = jobCode;
    public ActionPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a action plan metric in status created.</summary>
    public static ActionPlanMetric NewActionPlanMetric(string code, string title)
    {
        return new ActionPlanMetric(code, title, 0, "", "", ActionPlanMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanMetricStatus next)
    {
        switch (Status)
        {
            case ActionPlanMetricStatus.Created:
                switch (next)
                {
                    case ActionPlanMetricStatus.Published:
                        return true;
                }
                return false;
            case ActionPlanMetricStatus.Validated:
                return next == ActionPlanMetricStatus.Retired;
            case ActionPlanMetricStatus.Published:
                switch (next)
                {
                    case ActionPlanMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public ActionPlanMetric Copy()
    {
        return new ActionPlanMetric(Code, Title, Revision, Skill, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
