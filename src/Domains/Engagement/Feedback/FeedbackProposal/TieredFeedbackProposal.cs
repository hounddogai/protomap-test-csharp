namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered feedback proposal is in its life cycle.</summary>
public enum TieredFeedbackProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tiered feedback proposal, with what the planners need to act on it.</summary>
public sealed class TieredFeedbackProposal(string code, string name, string segment, string ownerId, string description, double hours, string currency, double margin, TieredFeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string OwnerId { get; set; } = ownerId;
    public string Description { get; set; } = description;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public TieredFeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a tiered feedback proposal in status idle.</summary>
    public static TieredFeedbackProposal NewTieredFeedbackProposal(string code, string name)
    {
        return new TieredFeedbackProposal(code, name, "", "", "", 0.0, "", 0.0, TieredFeedbackProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredFeedbackProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredFeedbackProposalStatus next)
    {
        switch (Status)
        {
            case TieredFeedbackProposalStatus.Idle:
                switch (next)
                {
                    case TieredFeedbackProposalStatus.Stopped:
                        return true;
                }
                return false;
            case TieredFeedbackProposalStatus.Warming:
                return next == TieredFeedbackProposalStatus.Ready || next == TieredFeedbackProposalStatus.Stopped;
            case TieredFeedbackProposalStatus.Ready:
                return next == TieredFeedbackProposalStatus.Draining || next == TieredFeedbackProposalStatus.Stopped;
            case TieredFeedbackProposalStatus.Draining:
                return next == TieredFeedbackProposalStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredFeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin - Hours, 2);
    }

    public TieredFeedbackProposal Copy()
    {
        return new TieredFeedbackProposal(
            Code,
            Name,
            Segment,
            OwnerId,
            Description,
            Hours,
            Currency,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
