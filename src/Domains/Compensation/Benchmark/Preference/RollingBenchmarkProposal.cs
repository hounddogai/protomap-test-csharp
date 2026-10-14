namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling benchmark proposal is in its life cycle.</summary>
public enum RollingBenchmarkProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling benchmark proposal. Its code is unique within a tenant.</summary>
public sealed class RollingBenchmarkProposal(string code, string name, string summary, double fte, string contactEmail, string description, string phone, string timezone, int capacity, int revision, string checksum, RollingBenchmarkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public string ContactEmail { get; set; } = contactEmail;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public int Capacity { get; set; } = capacity;
    public int Revision { get; set; } = revision;
    public string Checksum { get; set; } = checksum;
    public RollingBenchmarkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling benchmark proposal in status draft.</summary>
    public static RollingBenchmarkProposal NewRollingBenchmarkProposal(string code, string name)
    {
        return new RollingBenchmarkProposal(
            code,
            name,
            "",
            0.0,
            "",
            "",
            "",
            "",
            0,
            0,
            "",
            RollingBenchmarkProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBenchmarkProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBenchmarkProposalStatus next)
    {
        switch (Status)
        {
            case RollingBenchmarkProposalStatus.Draft:
                switch (next)
                {
                    case RollingBenchmarkProposalStatus.Pending:
                        return true;
                }
                return false;
            case RollingBenchmarkProposalStatus.Pending:
                switch (next)
                {
                    case RollingBenchmarkProposalStatus.Closed:
                        return true;
                }
                return false;
            case RollingBenchmarkProposalStatus.Active:
                switch (next)
                {
                    case RollingBenchmarkProposalStatus.Closed:
                        return true;
                }
                return false;
            case RollingBenchmarkProposalStatus.Suspended:
                return next == RollingBenchmarkProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingBenchmarkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling benchmark proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte + (double)Capacity, 2);
    }

    public RollingBenchmarkProposal Copy()
    {
        return new RollingBenchmarkProposal(
            Code,
            Name,
            Summary,
            Fte,
            ContactEmail,
            Description,
            Phone,
            Timezone,
            Capacity,
            Revision,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
