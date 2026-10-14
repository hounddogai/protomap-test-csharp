namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict career path proposal is in its life cycle.</summary>
public enum StrictCareerPathProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict career path proposal as the career path team keeps it.</summary>
public sealed class StrictCareerPathProposal(string code, string title, string country, int version, string badge, string employeeId, string externalRef, int tenureMonths, string jobCode, string grade, StrictCareerPathProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public string Badge { get; set; } = badge;
    public string EmployeeId { get; set; } = employeeId;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public StrictCareerPathProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict career path proposal in status draft.</summary>
    public static StrictCareerPathProposal NewStrictCareerPathProposal(string code, string title)
    {
        return new StrictCareerPathProposal(
            code,
            title,
            "",
            0,
            "",
            "",
            "",
            0,
            "",
            "",
            StrictCareerPathProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCareerPathProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCareerPathProposalStatus next)
    {
        switch (Status)
        {
            case StrictCareerPathProposalStatus.Draft:
                switch (next)
                {
                    case StrictCareerPathProposalStatus.Active:
                    case StrictCareerPathProposalStatus.Closed:
                        return true;
                }
                return false;
            case StrictCareerPathProposalStatus.Pending:
                switch (next)
                {
                    case StrictCareerPathProposalStatus.Suspended:
                        return true;
                }
                return false;
            case StrictCareerPathProposalStatus.Active:
                return next == StrictCareerPathProposalStatus.Closed;
            case StrictCareerPathProposalStatus.Suspended:
                switch (next)
                {
                    case StrictCareerPathProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictCareerPathProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict career path proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)TenureMonths * (double)Version, 2);
    }

    public StrictCareerPathProposal Copy()
    {
        return new StrictCareerPathProposal(
            Code,
            Title,
            Country,
            Version,
            Badge,
            EmployeeId,
            ExternalRef,
            TenureMonths,
            JobCode,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
