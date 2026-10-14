namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment snapshot is in its life cycle.</summary>
public enum SentimentSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A sentiment snapshot. Its code is unique within a tenant.</summary>
public sealed class SentimentSnapshot(string code, string title, string segment, double maximum, string summary, double cost, string name, string label, SentimentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public SentimentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment snapshot in status open.</summary>
    public static SentimentSnapshot NewSentimentSnapshot(string code, string title)
    {
        return new SentimentSnapshot(code, title, "", 0.0, "", 0.0, "", "", SentimentSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentSnapshotStatus next)
    {
        switch (Status)
        {
            case SentimentSnapshotStatus.Open:
                return next == SentimentSnapshotStatus.Archived;
            case SentimentSnapshotStatus.InReview:
                switch (next)
                {
                    case SentimentSnapshotStatus.Resolved:
                        return true;
                }
                return false;
            case SentimentSnapshotStatus.Resolved:
                return next == SentimentSnapshotStatus.Reopened;
            case SentimentSnapshotStatus.Reopened:
                return next == SentimentSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SentimentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Cost + Maximum, 2);
    }

    public SentimentSnapshot Copy()
    {
        return new SentimentSnapshot(Code, Title, Segment, Maximum, Summary, Cost, Name, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
