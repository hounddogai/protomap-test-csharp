namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global payslip proposal is in its life cycle.</summary>
public enum GlobalPayslipProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a global payslip proposal and its current status.</summary>
public sealed class GlobalPayslipProposal(string code, string title, double discount, int graceDays, int endsAt, double hours, int revision, GlobalPayslipProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int GraceDays { get; set; } = graceDays;
    public int EndsAt { get; set; } = endsAt;
    public double Hours { get; set; } = hours;
    public int Revision { get; set; } = revision;
    public GlobalPayslipProposalStatus Status { get; set; } = status;

    /// <summary>Starts a global payslip proposal in status created.</summary>
    public static GlobalPayslipProposal NewGlobalPayslipProposal(string code, string title)
    {
        return new GlobalPayslipProposal(code, title, 0.0, 0, 0, 0.0, 0, GlobalPayslipProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayslipProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayslipProposalStatus next)
    {
        switch (Status)
        {
            case GlobalPayslipProposalStatus.Created:
                return next == GlobalPayslipProposalStatus.Published || next == GlobalPayslipProposalStatus.Retired;
            case GlobalPayslipProposalStatus.Validated:
                switch (next)
                {
                    case GlobalPayslipProposalStatus.Retired:
                        return true;
                }
                return false;
            case GlobalPayslipProposalStatus.Published:
                switch (next)
                {
                    case GlobalPayslipProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPayslipProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global payslip proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"global payslip proposal"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Revision - Hours, 2);
    }

    public GlobalPayslipProposal Copy()
    {
        return new GlobalPayslipProposal(Code, Title, Discount, GraceDays, EndsAt, Hours, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
