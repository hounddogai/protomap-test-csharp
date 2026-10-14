namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant proposal is in its life cycle.</summary>
public enum EquityGrantProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One equity grant proposal, with what the planners need to act on it.</summary>
public sealed class EquityGrantProposal(string code, string name, int limit, string externalRef, string notes, double amount, EquityGrantProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public EquityGrantProposalStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant proposal in status draft.</summary>
    public static EquityGrantProposal NewEquityGrantProposal(string code, string name)
    {
        return new EquityGrantProposal(code, name, 0, "", "", 0.0, EquityGrantProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantProposalStatus next)
    {
        switch (Status)
        {
            case EquityGrantProposalStatus.Draft:
                return next == EquityGrantProposalStatus.Pending;
            case EquityGrantProposalStatus.Pending:
                return next == EquityGrantProposalStatus.Suspended;
            case EquityGrantProposalStatus.Active:
                return next == EquityGrantProposalStatus.Suspended || next == EquityGrantProposalStatus.Closed;
            case EquityGrantProposalStatus.Suspended:
                switch (next)
                {
                    case EquityGrantProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round(Amount + (double)Limit, 2);
    }

    public EquityGrantProposal Copy()
    {
        return new EquityGrantProposal(Code, Name, Limit, ExternalRef, Notes, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
