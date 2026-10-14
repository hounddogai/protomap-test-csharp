namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run proposal is in its life cycle.</summary>
public enum PayRunProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pay run proposal and its current status.</summary>
public sealed class PayRunProposal(string code, string label, string jobCode, int version, double minimum, int tenureMonths, string grade, double maximum, int limit, string notes, string source, PayRunProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public double Maximum { get; set; } = maximum;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public PayRunProposalStatus Status { get; set; } = status;

    /// <summary>Starts a pay run proposal in status proposed.</summary>
    public static PayRunProposal NewPayRunProposal(string code, string label)
    {
        return new PayRunProposal(code, label, "", 0, 0.0, 0, "", 0.0, 0, "", "", PayRunProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunProposalStatus next)
    {
        switch (Status)
        {
            case PayRunProposalStatus.Proposed:
                return next == PayRunProposalStatus.Approved;
            case PayRunProposalStatus.Approved:
                return next == PayRunProposalStatus.Withdrawn;
            case PayRunProposalStatus.Rejected:
                switch (next)
                {
                    case PayRunProposalStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Version - (double)TenureMonths, 2);
    }

    public PayRunProposal Copy()
    {
        return new PayRunProposal(
            Code,
            Label,
            JobCode,
            Version,
            Minimum,
            TenureMonths,
            Grade,
            Maximum,
            Limit,
            Notes,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
