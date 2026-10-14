namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance proposal is in its life cycle.</summary>
public enum BalanceProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A balance proposal. Its code is unique within a tenant.</summary>
public sealed class BalanceProposal(string code, string name, bool archived, int priority, string title, string managerId, BalanceProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int Priority { get; set; } = priority;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public BalanceProposalStatus Status { get; set; } = status;

    /// <summary>Starts a balance proposal in status open.</summary>
    public static BalanceProposal NewBalanceProposal(string code, string name)
    {
        return new BalanceProposal(code, name, false, 0, "", "", BalanceProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceProposalStatus next)
    {
        switch (Status)
        {
            case BalanceProposalStatus.Open:
                return next == BalanceProposalStatus.Resolved || next == BalanceProposalStatus.Archived;
            case BalanceProposalStatus.InReview:
                switch (next)
                {
                    case BalanceProposalStatus.Archived:
                        return true;
                }
                return false;
            case BalanceProposalStatus.Resolved:
                switch (next)
                {
                    case BalanceProposalStatus.Archived:
                        return true;
                }
                return false;
            case BalanceProposalStatus.Reopened:
                return next == BalanceProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BalanceProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public BalanceProposal Copy()
    {
        return new BalanceProposal(Code, Name, Archived, Priority, Title, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
