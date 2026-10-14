namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy ack plan is in its life cycle.</summary>
public enum PrimaryPolicyAckPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary policy ack plan, with what the planners need to act on it.</summary>
public sealed class PrimaryPolicyAckPlan(string code, string label, string badge, double minimum, string team, double ratio, bool approved, string title, PrimaryPolicyAckPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public PrimaryPolicyAckPlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy ack plan in status draft.</summary>
    public static PrimaryPolicyAckPlan NewPrimaryPolicyAckPlan(string code, string label)
    {
        return new PrimaryPolicyAckPlan(code, label, "", 0.0, "", 0.0, false, "", PrimaryPolicyAckPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyAckPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyAckPlanStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyAckPlanStatus.Draft:
                switch (next)
                {
                    case PrimaryPolicyAckPlanStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryPolicyAckPlanStatus.Pending:
                return next == PrimaryPolicyAckPlanStatus.Active;
            case PrimaryPolicyAckPlanStatus.Active:
                return next == PrimaryPolicyAckPlanStatus.Suspended || next == PrimaryPolicyAckPlanStatus.Closed;
            case PrimaryPolicyAckPlanStatus.Suspended:
                switch (next)
                {
                    case PrimaryPolicyAckPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyAckPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy ack plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum * Ratio, 2);
    }

    public PrimaryPolicyAckPlan Copy()
    {
        return new PrimaryPolicyAckPlan(Code, Label, Badge, Minimum, Team, Ratio, Approved, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
