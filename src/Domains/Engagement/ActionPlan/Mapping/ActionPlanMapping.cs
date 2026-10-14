namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan mapping is in its life cycle.</summary>
public enum ActionPlanMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A action plan mapping. Its code is unique within a tenant.</summary>
public sealed class ActionPlanMapping(string code, string title, string locale, int priority, int leaveDays, int endsAt, string managerId, double ceiling, ActionPlanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int Priority { get; set; } = priority;
    public int LeaveDays { get; set; } = leaveDays;
    public int EndsAt { get; set; } = endsAt;
    public string ManagerId { get; set; } = managerId;
    public double Ceiling { get; set; } = ceiling;
    public ActionPlanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a action plan mapping in status new.</summary>
    public static ActionPlanMapping NewActionPlanMapping(string code, string title)
    {
        return new ActionPlanMapping(code, title, "", 0, 0, 0, "", 0.0, ActionPlanMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanMappingStatus next)
    {
        switch (Status)
        {
            case ActionPlanMappingStatus.New:
                switch (next)
                {
                    case ActionPlanMappingStatus.Done:
                        return true;
                }
                return false;
            case ActionPlanMappingStatus.Queued:
                switch (next)
                {
                    case ActionPlanMappingStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanMappingStatus.Running:
                return next == ActionPlanMappingStatus.Done || next == ActionPlanMappingStatus.Failed;
            case ActionPlanMappingStatus.Done:
                return next == ActionPlanMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ActionPlanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)LeaveDays + (double)Priority, 2);
    }

    public ActionPlanMapping Copy()
    {
        return new ActionPlanMapping(Code, Title, Locale, Priority, LeaveDays, EndsAt, ManagerId, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
