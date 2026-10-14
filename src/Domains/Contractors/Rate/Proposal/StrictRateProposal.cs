namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict rate proposal is in its life cycle.</summary>
public enum StrictRateProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict rate proposal as the rate team keeps it.</summary>
public sealed class StrictRateProposal(string code, string label, string source, int version, string contactEmail, StrictRateProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public string ContactEmail { get; set; } = contactEmail;
    public StrictRateProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict rate proposal in status created.</summary>
    public static StrictRateProposal NewStrictRateProposal(string code, string label)
    {
        return new StrictRateProposal(code, label, "", 0, "", StrictRateProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRateProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRateProposalStatus next)
    {
        switch (Status)
        {
            case StrictRateProposalStatus.Created:
                switch (next)
                {
                    case StrictRateProposalStatus.Validated:
                    case StrictRateProposalStatus.Retired:
                        return true;
                }
                return false;
            case StrictRateProposalStatus.Validated:
                switch (next)
                {
                    case StrictRateProposalStatus.Published:
                    case StrictRateProposalStatus.Retired:
                        return true;
                }
                return false;
            case StrictRateProposalStatus.Published:
                switch (next)
                {
                    case StrictRateProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictRateProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict rate proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public StrictRateProposal Copy()
    {
        return new StrictRateProposal(Code, Label, Source, Version, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
