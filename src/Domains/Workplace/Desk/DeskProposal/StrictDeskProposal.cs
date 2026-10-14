namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict desk proposal is in its life cycle.</summary>
public enum StrictDeskProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One strict desk proposal, with what the planners need to act on it.</summary>
public sealed class StrictDeskProposal(string code, string title, int windowDays, string segment, string externalRef, string country, string location, string summary, double weight, bool approved, double rate, StrictDeskProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public string ExternalRef { get; set; } = externalRef;
    public string Country { get; set; } = country;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public bool Approved { get; set; } = approved;
    public double Rate { get; set; } = rate;
    public StrictDeskProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict desk proposal in status open.</summary>
    public static StrictDeskProposal NewStrictDeskProposal(string code, string title)
    {
        return new StrictDeskProposal(
            code,
            title,
            0,
            "",
            "",
            "",
            "",
            "",
            0.0,
            false,
            0.0,
            StrictDeskProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeskProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeskProposalStatus next)
    {
        switch (Status)
        {
            case StrictDeskProposalStatus.Open:
                switch (next)
                {
                    case StrictDeskProposalStatus.InReview:
                    case StrictDeskProposalStatus.Resolved:
                        return true;
                }
                return false;
            case StrictDeskProposalStatus.InReview:
                switch (next)
                {
                    case StrictDeskProposalStatus.Resolved:
                        return true;
                }
                return false;
            case StrictDeskProposalStatus.Resolved:
                return next == StrictDeskProposalStatus.Archived;
            case StrictDeskProposalStatus.Reopened:
                return next == StrictDeskProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictDeskProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict desk proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict desk proposal"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Weight * (double)WindowDays, 2);
    }

    public StrictDeskProposal Copy()
    {
        return new StrictDeskProposal(
            Code,
            Title,
            WindowDays,
            Segment,
            ExternalRef,
            Country,
            Location,
            Summary,
            Weight,
            Approved,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
