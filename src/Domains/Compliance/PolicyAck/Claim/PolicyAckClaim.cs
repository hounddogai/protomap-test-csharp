namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack claim is in its life cycle.</summary>
public enum PolicyAckClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a policy ack claim and its current status.</summary>
public sealed class PolicyAckClaim(string code, string title, string category, double amount, string location, string name, string externalRef, int tenureMonths, string segment, string phone, PolicyAckClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public string Location { get; set; } = location;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Segment { get; set; } = segment;
    public string Phone { get; set; } = phone;
    public PolicyAckClaimStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack claim in status draft.</summary>
    public static PolicyAckClaim NewPolicyAckClaim(string code, string title)
    {
        return new PolicyAckClaim(code, title, "", 0.0, "", "", "", 0, "", "", PolicyAckClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckClaimStatus next)
    {
        switch (Status)
        {
            case PolicyAckClaimStatus.Draft:
                switch (next)
                {
                    case PolicyAckClaimStatus.Suspended:
                    case PolicyAckClaimStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckClaimStatus.Pending:
                switch (next)
                {
                    case PolicyAckClaimStatus.Active:
                    case PolicyAckClaimStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckClaimStatus.Active:
                switch (next)
                {
                    case PolicyAckClaimStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckClaimStatus.Suspended:
                return next == PolicyAckClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyAckClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount - (double)TenureMonths, 2);
    }

    public PolicyAckClaim Copy()
    {
        return new PolicyAckClaim(
            Code,
            Title,
            Category,
            Amount,
            Location,
            Name,
            ExternalRef,
            TenureMonths,
            Segment,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
