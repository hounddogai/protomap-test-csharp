namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary mobility proposal is in its life cycle.</summary>
public enum PrimaryMobilityProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary mobility proposal. Its code is unique within a tenant.</summary>
public sealed class PrimaryMobilityProposal(string code, string name, string badge, List<string> tags, double minimum, string grade, bool archived, string channel, string jobCode, int createdAt, PrimaryMobilityProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public double Minimum { get; set; } = minimum;
    public string Grade { get; set; } = grade;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public PrimaryMobilityProposalStatus Status { get; set; } = status;

    /// <summary>Starts a primary mobility proposal in status draft.</summary>
    public static PrimaryMobilityProposal NewPrimaryMobilityProposal(string code, string name)
    {
        return new PrimaryMobilityProposal(
            code,
            name,
            "",
            new List<string>(),
            0.0,
            "",
            false,
            "",
            "",
            0,
            PrimaryMobilityProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMobilityProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMobilityProposalStatus next)
    {
        switch (Status)
        {
            case PrimaryMobilityProposalStatus.Draft:
                return next == PrimaryMobilityProposalStatus.Active;
            case PrimaryMobilityProposalStatus.Pending:
                return next == PrimaryMobilityProposalStatus.Suspended;
            case PrimaryMobilityProposalStatus.Active:
                return next == PrimaryMobilityProposalStatus.Suspended;
            case PrimaryMobilityProposalStatus.Suspended:
                return next == PrimaryMobilityProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryMobilityProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary mobility proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum + (double)CreatedAt, 2);
    }

    public PrimaryMobilityProposal Copy()
    {
        return new PrimaryMobilityProposal(
            Code,
            Name,
            Badge,
            Tags,
            Minimum,
            Grade,
            Archived,
            Channel,
            JobCode,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
