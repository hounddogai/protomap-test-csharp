namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan cycle is in its life cycle.</summary>
public enum ActionPlanCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A action plan cycle. Its code is unique within a tenant.</summary>
public sealed class ActionPlanCycle(string code, string name, string team, int tenureMonths, string contactEmail, string ownerId, ActionPlanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public ActionPlanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a action plan cycle in status idle.</summary>
    public static ActionPlanCycle NewActionPlanCycle(string code, string name)
    {
        return new ActionPlanCycle(code, name, "", 0, "", "", ActionPlanCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanCycleStatus next)
    {
        switch (Status)
        {
            case ActionPlanCycleStatus.Idle:
                return next == ActionPlanCycleStatus.Warming || next == ActionPlanCycleStatus.Stopped;
            case ActionPlanCycleStatus.Warming:
                return next == ActionPlanCycleStatus.Ready;
            case ActionPlanCycleStatus.Ready:
                switch (next)
                {
                    case ActionPlanCycleStatus.Stopped:
                        return true;
                }
                return false;
            case ActionPlanCycleStatus.Draining:
                return next == ActionPlanCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ActionPlanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"action plan cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public ActionPlanCycle Copy()
    {
        return new ActionPlanCycle(Code, Name, Team, TenureMonths, ContactEmail, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
