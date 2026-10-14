namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment draft is in its life cycle.</summary>
public enum SentimentDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A sentiment draft as the sentiment team keeps it.</summary>
public sealed class SentimentDraft(string code, string title, string location, string checksum, int capacity, string badge, string reviewer, SentimentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public string Reviewer { get; set; } = reviewer;
    public SentimentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment draft in status planned.</summary>
    public static SentimentDraft NewSentimentDraft(string code, string title)
    {
        return new SentimentDraft(code, title, "", "", 0, "", "", SentimentDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentDraftStatus next)
    {
        switch (Status)
        {
            case SentimentDraftStatus.Planned:
                return next == SentimentDraftStatus.Completed;
            case SentimentDraftStatus.Scheduled:
                return next == SentimentDraftStatus.Completed || next == SentimentDraftStatus.Cancelled;
            case SentimentDraftStatus.InProgress:
                return next == SentimentDraftStatus.Cancelled;
            case SentimentDraftStatus.Completed:
                return next == SentimentDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SentimentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public SentimentDraft Copy()
    {
        return new SentimentDraft(Code, Title, Location, Checksum, Capacity, Badge, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
