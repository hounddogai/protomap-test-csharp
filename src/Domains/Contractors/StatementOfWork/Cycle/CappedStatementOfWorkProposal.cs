namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped statement of work proposal is in its life cycle.</summary>
public enum CappedStatementOfWorkProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped statement of work proposal, with what the planners need to act on it.</summary>
public sealed class CappedStatementOfWorkProposal(string code, string name, int limit, double ceiling, string employeeId, double discount, string department, int batchSize, CappedStatementOfWorkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Ceiling { get; set; } = ceiling;
    public string EmployeeId { get; set; } = employeeId;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public CappedStatementOfWorkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a capped statement of work proposal in status proposed.</summary>
    public static CappedStatementOfWorkProposal NewCappedStatementOfWorkProposal(string code, string name)
    {
        return new CappedStatementOfWorkProposal(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            "",
            0,
            CappedStatementOfWorkProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedStatementOfWorkProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedStatementOfWorkProposalStatus next)
    {
        switch (Status)
        {
            case CappedStatementOfWorkProposalStatus.Proposed:
                switch (next)
                {
                    case CappedStatementOfWorkProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedStatementOfWorkProposalStatus.Approved:
                return next == CappedStatementOfWorkProposalStatus.Rejected || next == CappedStatementOfWorkProposalStatus.Withdrawn;
            case CappedStatementOfWorkProposalStatus.Rejected:
                return next == CappedStatementOfWorkProposalStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedStatementOfWorkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped statement of work proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Discount + Ceiling, 2);
    }

    public CappedStatementOfWorkProposal Copy()
    {
        return new CappedStatementOfWorkProposal(
            Code,
            Name,
            Limit,
            Ceiling,
            EmployeeId,
            Discount,
            Department,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
