namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan claim is in its life cycle.</summary>
public enum LenientSuccessionPlanClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient succession plan claim as the succession plan team keeps it.</summary>
public sealed class LenientSuccessionPlanClaim(string code, string label, string source, double floorValue, string costCenter, bool active, string contactEmail, string managerId, int leaveDays, double maximum, LenientSuccessionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double FloorValue { get; set; } = floorValue;
    public string CostCenter { get; set; } = costCenter;
    public bool Active { get; set; } = active;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public double Maximum { get; set; } = maximum;
    public LenientSuccessionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan claim in status proposed.</summary>
    public static LenientSuccessionPlanClaim NewLenientSuccessionPlanClaim(string code, string label)
    {
        return new LenientSuccessionPlanClaim(
            code,
            label,
            "",
            0.0,
            "",
            false,
            "",
            "",
            0,
            0.0,
            LenientSuccessionPlanClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanClaimStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanClaimStatus.Proposed:
                return next == LenientSuccessionPlanClaimStatus.Rejected || next == LenientSuccessionPlanClaimStatus.Withdrawn;
            case LenientSuccessionPlanClaimStatus.Approved:
                return next == LenientSuccessionPlanClaimStatus.Rejected;
            case LenientSuccessionPlanClaimStatus.Rejected:
                switch (next)
                {
                    case LenientSuccessionPlanClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue * (double)LeaveDays, 2);
    }

    public LenientSuccessionPlanClaim Copy()
    {
        return new LenientSuccessionPlanClaim(
            Code,
            Label,
            Source,
            FloorValue,
            CostCenter,
            Active,
            ContactEmail,
            ManagerId,
            LeaveDays,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
