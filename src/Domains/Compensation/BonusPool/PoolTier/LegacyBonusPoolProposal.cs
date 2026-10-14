namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy bonus pool proposal is in its life cycle.</summary>
public enum LegacyBonusPoolProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy bonus pool proposal. Its code is unique within a tenant.</summary>
public sealed class LegacyBonusPoolProposal(string code, string title, int limit, string badge, double threshold, string phone, int retries, string label, int createdAt, double score, string region, LegacyBonusPoolProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Badge { get; set; } = badge;
    public double Threshold { get; set; } = threshold;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Score { get; set; } = score;
    public string Region { get; set; } = region;
    public LegacyBonusPoolProposalStatus Status { get; set; } = status;

    /// <summary>Starts a legacy bonus pool proposal in status draft.</summary>
    public static LegacyBonusPoolProposal NewLegacyBonusPoolProposal(string code, string title)
    {
        return new LegacyBonusPoolProposal(
            code,
            title,
            0,
            "",
            0.0,
            "",
            0,
            "",
            0,
            0.0,
            "",
            LegacyBonusPoolProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBonusPoolProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBonusPoolProposalStatus next)
    {
        switch (Status)
        {
            case LegacyBonusPoolProposalStatus.Draft:
                return next == LegacyBonusPoolProposalStatus.Active || next == LegacyBonusPoolProposalStatus.Suspended;
            case LegacyBonusPoolProposalStatus.Pending:
                return next == LegacyBonusPoolProposalStatus.Active || next == LegacyBonusPoolProposalStatus.Suspended;
            case LegacyBonusPoolProposalStatus.Active:
                switch (next)
                {
                    case LegacyBonusPoolProposalStatus.Suspended:
                    case LegacyBonusPoolProposalStatus.Closed:
                        return true;
                }
                return false;
            case LegacyBonusPoolProposalStatus.Suspended:
                return next == LegacyBonusPoolProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyBonusPoolProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy bonus pool proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Limit * (double)Retries, 2);
    }

    public LegacyBonusPoolProposal Copy()
    {
        return new LegacyBonusPoolProposal(
            Code,
            Title,
            Limit,
            Badge,
            Threshold,
            Phone,
            Retries,
            Label,
            CreatedAt,
            Score,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
