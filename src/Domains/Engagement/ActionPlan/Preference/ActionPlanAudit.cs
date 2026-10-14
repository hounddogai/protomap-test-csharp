namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan audit is in its life cycle.</summary>
public enum ActionPlanAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a action plan audit and its current status.</summary>
public sealed class ActionPlanAudit(string code, string title, string country, int revision, string currency, string channel, ActionPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public int Revision { get; set; } = revision;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public ActionPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a action plan audit in status new.</summary>
    public static ActionPlanAudit NewActionPlanAudit(string code, string title)
    {
        return new ActionPlanAudit(code, title, "", 0, "", "", ActionPlanAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanAuditStatus next)
    {
        switch (Status)
        {
            case ActionPlanAuditStatus.New:
                return next == ActionPlanAuditStatus.Done;
            case ActionPlanAuditStatus.Queued:
                return next == ActionPlanAuditStatus.Running;
            case ActionPlanAuditStatus.Running:
                switch (next)
                {
                    case ActionPlanAuditStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanAuditStatus.Done:
                switch (next)
                {
                    case ActionPlanAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public ActionPlanAudit Copy()
    {
        return new ActionPlanAudit(Code, Title, Country, Revision, Currency, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
