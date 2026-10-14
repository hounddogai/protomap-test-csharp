namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy ack proposal is in its life cycle.</summary>
public enum AdaptivePolicyAckProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive policy ack proposal as the policy ack team keeps it.</summary>
public sealed class AdaptivePolicyAckProposal(string code, string name, string salaryBand, string badge, string description, int expiresAt, int version, double weight, int retries, int limit, double margin, AdaptivePolicyAckProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public double Weight { get; set; } = weight;
    public int Retries { get; set; } = retries;
    public int Limit { get; set; } = limit;
    public double Margin { get; set; } = margin;
    public AdaptivePolicyAckProposalStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy ack proposal in status open.</summary>
    public static AdaptivePolicyAckProposal NewAdaptivePolicyAckProposal(string code, string name)
    {
        return new AdaptivePolicyAckProposal(
            code,
            name,
            "",
            "",
            "",
            0,
            0,
            0.0,
            0,
            0,
            0.0,
            AdaptivePolicyAckProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyAckProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyAckProposalStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyAckProposalStatus.Open:
                switch (next)
                {
                    case AdaptivePolicyAckProposalStatus.InReview:
                        return true;
                }
                return false;
            case AdaptivePolicyAckProposalStatus.InReview:
                switch (next)
                {
                    case AdaptivePolicyAckProposalStatus.Resolved:
                    case AdaptivePolicyAckProposalStatus.Archived:
                        return true;
                }
                return false;
            case AdaptivePolicyAckProposalStatus.Resolved:
                return next == AdaptivePolicyAckProposalStatus.Reopened || next == AdaptivePolicyAckProposalStatus.Archived;
            case AdaptivePolicyAckProposalStatus.Reopened:
                return next == AdaptivePolicyAckProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyAckProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy ack proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"adaptive policy ack proposal"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin + (double)Version, 2);
    }

    public AdaptivePolicyAckProposal Copy()
    {
        return new AdaptivePolicyAckProposal(
            Code,
            Name,
            SalaryBand,
            Badge,
            Description,
            ExpiresAt,
            Version,
            Weight,
            Retries,
            Limit,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
