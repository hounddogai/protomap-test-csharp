namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate proposal is in its life cycle.</summary>
public enum RateProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rate proposal. Its code is unique within a tenant.</summary>
public sealed class RateProposal(string code, string title, string salaryBand, int tenureMonths, int capacity, string currency, string name, string employeeId, int createdAt, RateProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int CreatedAt { get; set; } = createdAt;
    public RateProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rate proposal in status draft.</summary>
    public static RateProposal NewRateProposal(string code, string title)
    {
        return new RateProposal(code, title, "", 0, 0, "", "", "", 0, RateProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateProposalStatus next)
    {
        switch (Status)
        {
            case RateProposalStatus.Draft:
                switch (next)
                {
                    case RateProposalStatus.Suspended:
                        return true;
                }
                return false;
            case RateProposalStatus.Pending:
                switch (next)
                {
                    case RateProposalStatus.Suspended:
                        return true;
                }
                return false;
            case RateProposalStatus.Active:
                switch (next)
                {
                    case RateProposalStatus.Suspended:
                    case RateProposalStatus.Closed:
                        return true;
                }
                return false;
            case RateProposalStatus.Suspended:
                return next == RateProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RateProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)TenureMonths * (double)Capacity, 2);
    }

    public RateProposal Copy()
    {
        return new RateProposal(
            Code,
            Title,
            SalaryBand,
            TenureMonths,
            Capacity,
            Currency,
            Name,
            EmployeeId,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
