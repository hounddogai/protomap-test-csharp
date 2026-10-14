namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool proposal is in its life cycle.</summary>
public enum BonusPoolProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A bonus pool proposal. Its code is unique within a tenant.</summary>
public sealed class BonusPoolProposal(string code, string title, int retries, bool approved, int leaveDays, bool active, string badge, double rate, BonusPoolProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public BonusPoolProposalStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool proposal in status created.</summary>
    public static BonusPoolProposal NewBonusPoolProposal(string code, string title)
    {
        return new BonusPoolProposal(code, title, 0, false, 0, false, "", 0.0, BonusPoolProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolProposalStatus next)
    {
        switch (Status)
        {
            case BonusPoolProposalStatus.Created:
                switch (next)
                {
                    case BonusPoolProposalStatus.Validated:
                    case BonusPoolProposalStatus.Published:
                        return true;
                }
                return false;
            case BonusPoolProposalStatus.Validated:
                switch (next)
                {
                    case BonusPoolProposalStatus.Published:
                    case BonusPoolProposalStatus.Retired:
                        return true;
                }
                return false;
            case BonusPoolProposalStatus.Published:
                switch (next)
                {
                    case BonusPoolProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays * Rate, 2);
    }

    public BonusPoolProposal Copy()
    {
        return new BonusPoolProposal(Code, Title, Retries, Approved, LeaveDays, Active, Badge, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
