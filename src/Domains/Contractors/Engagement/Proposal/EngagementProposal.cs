namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement proposal is in its life cycle.</summary>
public enum EngagementProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A engagement proposal. Its code is unique within a tenant.</summary>
public sealed class EngagementProposal(string code, string name, bool archived, string description, string managerId, bool visible, double amount, EngagementProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public string ManagerId { get; set; } = managerId;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public EngagementProposalStatus Status { get; set; } = status;

    /// <summary>Starts a engagement proposal in status requested.</summary>
    public static EngagementProposal NewEngagementProposal(string code, string name)
    {
        return new EngagementProposal(code, name, false, "", "", false, 0.0, EngagementProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementProposalStatus next)
    {
        switch (Status)
        {
            case EngagementProposalStatus.Requested:
                switch (next)
                {
                    case EngagementProposalStatus.Revoked:
                        return true;
                }
                return false;
            case EngagementProposalStatus.Granted:
                switch (next)
                {
                    case EngagementProposalStatus.Revoked:
                        return true;
                }
                return false;
            case EngagementProposalStatus.Revoked:
                return next == EngagementProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(EngagementProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public EngagementProposal Copy()
    {
        return new EngagementProposal(Code, Name, Archived, Description, ManagerId, Visible, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
