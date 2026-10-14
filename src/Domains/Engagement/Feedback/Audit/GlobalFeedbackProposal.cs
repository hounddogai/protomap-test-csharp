namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global feedback proposal is in its life cycle.</summary>
public enum GlobalFeedbackProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global feedback proposal. Its code is unique within a tenant.</summary>
public sealed class GlobalFeedbackProposal(string code, string name, double ratio, string contactEmail, double minimum, int endsAt, int createdAt, string category, string grade, List<string> tags, GlobalFeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public string Grade { get; set; } = grade;
    public List<string> Tags { get; set; } = tags;
    public GlobalFeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a global feedback proposal in status open.</summary>
    public static GlobalFeedbackProposal NewGlobalFeedbackProposal(string code, string name)
    {
        return new GlobalFeedbackProposal(
            code,
            name,
            0.0,
            "",
            0.0,
            0,
            0,
            "",
            "",
            new List<string>(),
            GlobalFeedbackProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalFeedbackProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalFeedbackProposalStatus next)
    {
        switch (Status)
        {
            case GlobalFeedbackProposalStatus.Open:
                return next == GlobalFeedbackProposalStatus.Reopened || next == GlobalFeedbackProposalStatus.Archived;
            case GlobalFeedbackProposalStatus.InReview:
                switch (next)
                {
                    case GlobalFeedbackProposalStatus.Resolved:
                        return true;
                }
                return false;
            case GlobalFeedbackProposalStatus.Resolved:
                return next == GlobalFeedbackProposalStatus.Reopened || next == GlobalFeedbackProposalStatus.Archived;
            case GlobalFeedbackProposalStatus.Reopened:
                return next == GlobalFeedbackProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalFeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global feedback proposal"} {Code}";
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

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt + (double)EndsAt, 2);
    }

    public GlobalFeedbackProposal Copy()
    {
        return new GlobalFeedbackProposal(
            Code,
            Name,
            Ratio,
            ContactEmail,
            Minimum,
            EndsAt,
            CreatedAt,
            Category,
            Grade,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
