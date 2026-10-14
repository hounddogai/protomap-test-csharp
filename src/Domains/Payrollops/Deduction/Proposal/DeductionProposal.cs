namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction proposal is in its life cycle.</summary>
public enum DeductionProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A deduction proposal as the deduction team keeps it.</summary>
public sealed class DeductionProposal(string code, string name, string jobCode, string grade, string managerId, string team, string notes, string badge, double rating, int limit, double amount, DeductionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public string Notes { get; set; } = notes;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public double Amount { get; set; } = amount;
    public DeductionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a deduction proposal in status proposed.</summary>
    public static DeductionProposal NewDeductionProposal(string code, string name)
    {
        return new DeductionProposal(
            code,
            name,
            "",
            "",
            "",
            "",
            "",
            "",
            0.0,
            0,
            0.0,
            DeductionProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionProposalStatus next)
    {
        switch (Status)
        {
            case DeductionProposalStatus.Proposed:
                return next == DeductionProposalStatus.Rejected || next == DeductionProposalStatus.Withdrawn;
            case DeductionProposalStatus.Approved:
                switch (next)
                {
                    case DeductionProposalStatus.Rejected:
                    case DeductionProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionProposalStatus.Rejected:
                switch (next)
                {
                    case DeductionProposalStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Limit * Rating, 2);
    }

    public DeductionProposal Copy()
    {
        return new DeductionProposal(
            Code,
            Name,
            JobCode,
            Grade,
            ManagerId,
            Team,
            Notes,
            Badge,
            Rating,
            Limit,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
