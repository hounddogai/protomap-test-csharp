namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global succession plan request is in its life cycle.</summary>
public enum GlobalSuccessionPlanRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global succession plan request as the succession plan team keeps it.</summary>
public sealed class GlobalSuccessionPlanRequest(string code, string label, int priority, string managerId, double cost, string unit, GlobalSuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public GlobalSuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a global succession plan request in status draft.</summary>
    public static GlobalSuccessionPlanRequest NewGlobalSuccessionPlanRequest(string code, string label)
    {
        return new GlobalSuccessionPlanRequest(code, label, 0, "", 0.0, "", GlobalSuccessionPlanRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSuccessionPlanRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case GlobalSuccessionPlanRequestStatus.Draft:
                switch (next)
                {
                    case GlobalSuccessionPlanRequestStatus.Closed:
                        return true;
                }
                return false;
            case GlobalSuccessionPlanRequestStatus.Pending:
                return next == GlobalSuccessionPlanRequestStatus.Active || next == GlobalSuccessionPlanRequestStatus.Suspended;
            case GlobalSuccessionPlanRequestStatus.Active:
                return next == GlobalSuccessionPlanRequestStatus.Suspended || next == GlobalSuccessionPlanRequestStatus.Closed;
            case GlobalSuccessionPlanRequestStatus.Suspended:
                return next == GlobalSuccessionPlanRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalSuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority + Cost, 2);
    }

    public GlobalSuccessionPlanRequest Copy()
    {
        return new GlobalSuccessionPlanRequest(Code, Label, Priority, ManagerId, Cost, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
