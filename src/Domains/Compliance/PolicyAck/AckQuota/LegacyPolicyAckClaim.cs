namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy policy ack claim is in its life cycle.</summary>
public enum LegacyPolicyAckClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One legacy policy ack claim, with what the planners need to act on it.</summary>
public sealed class LegacyPolicyAckClaim(string code, string label, string ownerId, string salaryBand, int capacity, double rating, double ceiling, string currency, string badge, string title, int attempts, LegacyPolicyAckClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string SalaryBand { get; set; } = salaryBand;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public string Currency { get; set; } = currency;
    public string Badge { get; set; } = badge;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public LegacyPolicyAckClaimStatus Status { get; set; } = status;

    /// <summary>Starts a legacy policy ack claim in status new.</summary>
    public static LegacyPolicyAckClaim NewLegacyPolicyAckClaim(string code, string label)
    {
        return new LegacyPolicyAckClaim(
            code,
            label,
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            "",
            "",
            0,
            LegacyPolicyAckClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPolicyAckClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPolicyAckClaimStatus next)
    {
        switch (Status)
        {
            case LegacyPolicyAckClaimStatus.New:
                switch (next)
                {
                    case LegacyPolicyAckClaimStatus.Failed:
                        return true;
                }
                return false;
            case LegacyPolicyAckClaimStatus.Queued:
                switch (next)
                {
                    case LegacyPolicyAckClaimStatus.Done:
                    case LegacyPolicyAckClaimStatus.Failed:
                        return true;
                }
                return false;
            case LegacyPolicyAckClaimStatus.Running:
                return next == LegacyPolicyAckClaimStatus.Done || next == LegacyPolicyAckClaimStatus.Failed;
            case LegacyPolicyAckClaimStatus.Done:
                switch (next)
                {
                    case LegacyPolicyAckClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPolicyAckClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy policy ack claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Attempts * Rating, 2);
    }

    public LegacyPolicyAckClaim Copy()
    {
        return new LegacyPolicyAckClaim(
            Code,
            Label,
            OwnerId,
            SalaryBand,
            Capacity,
            Rating,
            Ceiling,
            Currency,
            Badge,
            Title,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
