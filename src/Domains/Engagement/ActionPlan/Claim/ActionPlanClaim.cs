namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan claim is in its life cycle.</summary>
public enum ActionPlanClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A action plan claim as the action plan team keeps it.</summary>
public sealed class ActionPlanClaim(string code, string name, double hours, double ceiling, double cost, ActionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public ActionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a action plan claim in status new.</summary>
    public static ActionPlanClaim NewActionPlanClaim(string code, string name)
    {
        return new ActionPlanClaim(code, name, 0.0, 0.0, 0.0, ActionPlanClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanClaimStatus next)
    {
        switch (Status)
        {
            case ActionPlanClaimStatus.New:
                switch (next)
                {
                    case ActionPlanClaimStatus.Queued:
                        return true;
                }
                return false;
            case ActionPlanClaimStatus.Queued:
                switch (next)
                {
                    case ActionPlanClaimStatus.Done:
                    case ActionPlanClaimStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanClaimStatus.Running:
                switch (next)
                {
                    case ActionPlanClaimStatus.Done:
                    case ActionPlanClaimStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanClaimStatus.Done:
                switch (next)
                {
                    case ActionPlanClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Hours - Cost, 2);
    }

    public ActionPlanClaim Copy()
    {
        return new ActionPlanClaim(Code, Name, Hours, Ceiling, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
