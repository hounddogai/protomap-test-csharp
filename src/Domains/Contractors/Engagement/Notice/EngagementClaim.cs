namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement claim is in its life cycle.</summary>
public enum EngagementClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a engagement claim and its current status.</summary>
public sealed class EngagementClaim(string code, string label, string externalRef, double discount, double rating, string managerId, double cost, EngagementClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public EngagementClaimStatus Status { get; set; } = status;

    /// <summary>Starts a engagement claim in status proposed.</summary>
    public static EngagementClaim NewEngagementClaim(string code, string label)
    {
        return new EngagementClaim(code, label, "", 0.0, 0.0, "", 0.0, EngagementClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementClaimStatus next)
    {
        switch (Status)
        {
            case EngagementClaimStatus.Proposed:
                switch (next)
                {
                    case EngagementClaimStatus.Rejected:
                    case EngagementClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case EngagementClaimStatus.Approved:
                return next == EngagementClaimStatus.Rejected || next == EngagementClaimStatus.Withdrawn;
            case EngagementClaimStatus.Rejected:
                switch (next)
                {
                    case EngagementClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount - Rating, 2);
    }

    public EngagementClaim Copy()
    {
        return new EngagementClaim(Code, Label, ExternalRef, Discount, Rating, ManagerId, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
