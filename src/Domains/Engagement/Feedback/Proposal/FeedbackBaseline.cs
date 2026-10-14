namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback baseline is in its life cycle.</summary>
public enum FeedbackBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A feedback baseline as the feedback team keeps it.</summary>
public sealed class FeedbackBaseline(string code, string title, string summary, double fte, int updatedAt, int sequence, string team, string badge, string segment, int version, int endsAt, FeedbackBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Sequence { get; set; } = sequence;
    public string Team { get; set; } = team;
    public string Badge { get; set; } = badge;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public FeedbackBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a feedback baseline in status created.</summary>
    public static FeedbackBaseline NewFeedbackBaseline(string code, string title)
    {
        return new FeedbackBaseline(code, title, "", 0.0, 0, 0, "", "", "", 0, 0, FeedbackBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackBaselineStatus next)
    {
        switch (Status)
        {
            case FeedbackBaselineStatus.Created:
                return next == FeedbackBaselineStatus.Validated || next == FeedbackBaselineStatus.Published;
            case FeedbackBaselineStatus.Validated:
                switch (next)
                {
                    case FeedbackBaselineStatus.Published:
                    case FeedbackBaselineStatus.Retired:
                        return true;
                }
                return false;
            case FeedbackBaselineStatus.Published:
                return next == FeedbackBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FeedbackBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt * (double)Sequence, 2);
    }

    public FeedbackBaseline Copy()
    {
        return new FeedbackBaseline(
            Code,
            Title,
            Summary,
            Fte,
            UpdatedAt,
            Sequence,
            Team,
            Badge,
            Segment,
            Version,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
