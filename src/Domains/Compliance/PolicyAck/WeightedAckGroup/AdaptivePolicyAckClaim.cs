namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy ack claim is in its life cycle.</summary>
public enum AdaptivePolicyAckClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive policy ack claim. Its code is unique within a tenant.</summary>
public sealed class AdaptivePolicyAckClaim(string code, string label, int leaveDays, int graceDays, int limit, string source, string managerId, int tenureMonths, string location, AdaptivePolicyAckClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public string Source { get; set; } = source;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public AdaptivePolicyAckClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy ack claim in status proposed.</summary>
    public static AdaptivePolicyAckClaim NewAdaptivePolicyAckClaim(string code, string label)
    {
        return new AdaptivePolicyAckClaim(
            code,
            label,
            0,
            0,
            0,
            "",
            "",
            0,
            "",
            AdaptivePolicyAckClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyAckClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyAckClaimStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyAckClaimStatus.Proposed:
                return next == AdaptivePolicyAckClaimStatus.Approved || next == AdaptivePolicyAckClaimStatus.Rejected;
            case AdaptivePolicyAckClaimStatus.Approved:
                return next == AdaptivePolicyAckClaimStatus.Rejected || next == AdaptivePolicyAckClaimStatus.Withdrawn;
            case AdaptivePolicyAckClaimStatus.Rejected:
                switch (next)
                {
                    case AdaptivePolicyAckClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyAckClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy ack claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive policy ack claim"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)LeaveDays + (double)TenureMonths, 2);
    }

    public AdaptivePolicyAckClaim Copy()
    {
        return new AdaptivePolicyAckClaim(
            Code,
            Label,
            LeaveDays,
            GraceDays,
            Limit,
            Source,
            ManagerId,
            TenureMonths,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
