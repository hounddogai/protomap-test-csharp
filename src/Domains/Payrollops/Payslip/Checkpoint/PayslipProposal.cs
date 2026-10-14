namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip proposal is in its life cycle.</summary>
public enum PayslipProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip proposal. Its code is unique within a tenant.</summary>
public sealed class PayslipProposal(string code, string title, string skill, double budget, double minimum, string ownerId, double discount, PayslipProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public PayslipProposalStatus Status { get; set; } = status;

    /// <summary>Starts a payslip proposal in status requested.</summary>
    public static PayslipProposal NewPayslipProposal(string code, string title)
    {
        return new PayslipProposal(code, title, "", 0.0, 0.0, "", 0.0, PayslipProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipProposalStatus next)
    {
        switch (Status)
        {
            case PayslipProposalStatus.Requested:
                return next == PayslipProposalStatus.Expired;
            case PayslipProposalStatus.Granted:
                switch (next)
                {
                    case PayslipProposalStatus.Expired:
                        return true;
                }
                return false;
            case PayslipProposalStatus.Revoked:
                switch (next)
                {
                    case PayslipProposalStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip proposal"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Discount + Minimum, 2);
    }

    public PayslipProposal Copy()
    {
        return new PayslipProposal(Code, Title, Skill, Budget, Minimum, OwnerId, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
