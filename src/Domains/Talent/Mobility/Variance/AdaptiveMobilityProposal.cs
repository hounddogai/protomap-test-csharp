namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive mobility proposal is in its life cycle.</summary>
public enum AdaptiveMobilityProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One adaptive mobility proposal, with what the planners need to act on it.</summary>
public sealed class AdaptiveMobilityProposal(string code, string name, int batchSize, string country, double floorValue, string currency, string externalRef, double maximum, AdaptiveMobilityProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Country { get; set; } = country;
    public double FloorValue { get; set; } = floorValue;
    public string Currency { get; set; } = currency;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public AdaptiveMobilityProposalStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive mobility proposal in status draft.</summary>
    public static AdaptiveMobilityProposal NewAdaptiveMobilityProposal(string code, string name)
    {
        return new AdaptiveMobilityProposal(
            code,
            name,
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            AdaptiveMobilityProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveMobilityProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveMobilityProposalStatus next)
    {
        switch (Status)
        {
            case AdaptiveMobilityProposalStatus.Draft:
                switch (next)
                {
                    case AdaptiveMobilityProposalStatus.Pending:
                    case AdaptiveMobilityProposalStatus.Active:
                        return true;
                }
                return false;
            case AdaptiveMobilityProposalStatus.Pending:
                switch (next)
                {
                    case AdaptiveMobilityProposalStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveMobilityProposalStatus.Active:
                switch (next)
                {
                    case AdaptiveMobilityProposalStatus.Suspended:
                    case AdaptiveMobilityProposalStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveMobilityProposalStatus.Suspended:
                return next == AdaptiveMobilityProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveMobilityProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive mobility proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive mobility proposal"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Maximum + FloorValue, 2);
    }

    public AdaptiveMobilityProposal Copy()
    {
        return new AdaptiveMobilityProposal(
            Code,
            Name,
            BatchSize,
            Country,
            FloorValue,
            Currency,
            ExternalRef,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
