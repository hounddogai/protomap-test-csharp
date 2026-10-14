namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan request is in its life cycle.</summary>
public enum ActionPlanRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One action plan request, with what the planners need to act on it.</summary>
public sealed class ActionPlanRequest(string code, string name, int version, int startsAt, string department, int attempts, bool visible, ActionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public string Department { get; set; } = department;
    public int Attempts { get; set; } = attempts;
    public bool Visible { get; set; } = visible;
    public ActionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a action plan request in status draft.</summary>
    public static ActionPlanRequest NewActionPlanRequest(string code, string name)
    {
        return new ActionPlanRequest(code, name, 0, 0, "", 0, false, ActionPlanRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanRequestStatus next)
    {
        switch (Status)
        {
            case ActionPlanRequestStatus.Draft:
                return next == ActionPlanRequestStatus.Active || next == ActionPlanRequestStatus.Suspended;
            case ActionPlanRequestStatus.Pending:
                return next == ActionPlanRequestStatus.Closed;
            case ActionPlanRequestStatus.Active:
                return next == ActionPlanRequestStatus.Suspended || next == ActionPlanRequestStatus.Closed;
            case ActionPlanRequestStatus.Suspended:
                return next == ActionPlanRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ActionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Version * (double)Attempts, 2);
    }

    public ActionPlanRequest Copy()
    {
        return new ActionPlanRequest(Code, Name, Version, StartsAt, Department, Attempts, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
