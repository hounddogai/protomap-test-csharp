namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict succession plan claim is in its life cycle.</summary>
public enum StrictSuccessionPlanClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict succession plan claim and its current status.</summary>
public sealed class StrictSuccessionPlanClaim(string code, string label, string skill, string ownerId, int tenureMonths, StrictSuccessionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public StrictSuccessionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a strict succession plan claim in status idle.</summary>
    public static StrictSuccessionPlanClaim NewStrictSuccessionPlanClaim(string code, string label)
    {
        return new StrictSuccessionPlanClaim(code, label, "", "", 0, StrictSuccessionPlanClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSuccessionPlanClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSuccessionPlanClaimStatus next)
    {
        switch (Status)
        {
            case StrictSuccessionPlanClaimStatus.Idle:
                switch (next)
                {
                    case StrictSuccessionPlanClaimStatus.Warming:
                        return true;
                }
                return false;
            case StrictSuccessionPlanClaimStatus.Warming:
                return next == StrictSuccessionPlanClaimStatus.Ready || next == StrictSuccessionPlanClaimStatus.Draining;
            case StrictSuccessionPlanClaimStatus.Ready:
                switch (next)
                {
                    case StrictSuccessionPlanClaimStatus.Draining:
                    case StrictSuccessionPlanClaimStatus.Stopped:
                        return true;
                }
                return false;
            case StrictSuccessionPlanClaimStatus.Draining:
                return next == StrictSuccessionPlanClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictSuccessionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict succession plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public StrictSuccessionPlanClaim Copy()
    {
        return new StrictSuccessionPlanClaim(Code, Label, Skill, OwnerId, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
