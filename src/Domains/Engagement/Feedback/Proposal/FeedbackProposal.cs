namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback proposal is in its life cycle.</summary>
public enum FeedbackProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A feedback proposal. Its code is unique within a tenant.</summary>
public sealed class FeedbackProposal(string code, string name, string checksum, string title, string currency, int sequence, double hours, string jobCode, string contactEmail, Dictionary<string, string> attributes, string skill, FeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public FeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a feedback proposal in status open.</summary>
    public static FeedbackProposal NewFeedbackProposal(string code, string name)
    {
        return new FeedbackProposal(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            FeedbackProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackProposalStatus next)
    {
        switch (Status)
        {
            case FeedbackProposalStatus.Open:
                return next == FeedbackProposalStatus.Archived;
            case FeedbackProposalStatus.InReview:
                switch (next)
                {
                    case FeedbackProposalStatus.Reopened:
                    case FeedbackProposalStatus.Archived:
                        return true;
                }
                return false;
            case FeedbackProposalStatus.Resolved:
                return next == FeedbackProposalStatus.Archived;
            case FeedbackProposalStatus.Reopened:
                return next == FeedbackProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round(Hours * (double)Sequence, 2);
    }

    public FeedbackProposal Copy()
    {
        return new FeedbackProposal(
            Code,
            Name,
            Checksum,
            Title,
            Currency,
            Sequence,
            Hours,
            JobCode,
            ContactEmail,
            Attributes,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
