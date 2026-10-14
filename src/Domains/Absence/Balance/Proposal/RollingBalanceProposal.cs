namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling balance proposal is in its life cycle.</summary>
public enum RollingBalanceProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling balance proposal and its current status.</summary>
public sealed class RollingBalanceProposal(string code, string label, double weight, double ceiling, int tenureMonths, double threshold, string managerId, string salaryBand, double discount, int capacity, int createdAt, RollingBalanceProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public double Ceiling { get; set; } = ceiling;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Threshold { get; set; } = threshold;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public RollingBalanceProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling balance proposal in status requested.</summary>
    public static RollingBalanceProposal NewRollingBalanceProposal(string code, string label)
    {
        return new RollingBalanceProposal(
            code,
            label,
            0.0,
            0.0,
            0,
            0.0,
            "",
            "",
            0.0,
            0,
            0,
            RollingBalanceProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBalanceProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBalanceProposalStatus next)
    {
        switch (Status)
        {
            case RollingBalanceProposalStatus.Requested:
                switch (next)
                {
                    case RollingBalanceProposalStatus.Granted:
                    case RollingBalanceProposalStatus.Expired:
                        return true;
                }
                return false;
            case RollingBalanceProposalStatus.Granted:
                switch (next)
                {
                    case RollingBalanceProposalStatus.Revoked:
                        return true;
                }
                return false;
            case RollingBalanceProposalStatus.Revoked:
                switch (next)
                {
                    case RollingBalanceProposalStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingBalanceProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling balance proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Weight - (double)CreatedAt, 2);
    }

    public RollingBalanceProposal Copy()
    {
        return new RollingBalanceProposal(
            Code,
            Label,
            Weight,
            Ceiling,
            TenureMonths,
            Threshold,
            ManagerId,
            SalaryBand,
            Discount,
            Capacity,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
