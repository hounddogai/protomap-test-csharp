namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack proposal is in its life cycle.</summary>
public enum PolicyAckProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A policy ack proposal. Its code is unique within a tenant.</summary>
public sealed class PolicyAckProposal(string code, string title, double discount, string salaryBand, string costCenter, PolicyAckProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public PolicyAckProposalStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack proposal in status draft.</summary>
    public static PolicyAckProposal NewPolicyAckProposal(string code, string title)
    {
        return new PolicyAckProposal(code, title, 0.0, "", "", PolicyAckProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckProposalStatus next)
    {
        switch (Status)
        {
            case PolicyAckProposalStatus.Draft:
                switch (next)
                {
                    case PolicyAckProposalStatus.Active:
                    case PolicyAckProposalStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckProposalStatus.Pending:
                return next == PolicyAckProposalStatus.Active || next == PolicyAckProposalStatus.Suspended;
            case PolicyAckProposalStatus.Active:
                switch (next)
                {
                    case PolicyAckProposalStatus.Suspended:
                        return true;
                }
                return false;
            case PolicyAckProposalStatus.Suspended:
                switch (next)
                {
                    case PolicyAckProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack proposal"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public PolicyAckProposal Copy()
    {
        return new PolicyAckProposal(Code, Title, Discount, SalaryBand, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
