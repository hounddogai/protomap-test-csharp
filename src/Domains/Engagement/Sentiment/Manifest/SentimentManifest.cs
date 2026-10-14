namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment manifest is in its life cycle.</summary>
public enum SentimentManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One sentiment manifest, with what the planners need to act on it.</summary>
public sealed class SentimentManifest(string code, string label, string category, string externalRef, string segment, double cost, int batchSize, string costCenter, SentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public SentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment manifest in status planned.</summary>
    public static SentimentManifest NewSentimentManifest(string code, string label)
    {
        return new SentimentManifest(code, label, "", "", "", 0.0, 0, "", SentimentManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentManifestStatus next)
    {
        switch (Status)
        {
            case SentimentManifestStatus.Planned:
                switch (next)
                {
                    case SentimentManifestStatus.Scheduled:
                    case SentimentManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case SentimentManifestStatus.Scheduled:
                return next == SentimentManifestStatus.Cancelled;
            case SentimentManifestStatus.InProgress:
                return next == SentimentManifestStatus.Cancelled;
            case SentimentManifestStatus.Completed:
                switch (next)
                {
                    case SentimentManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost - (double)BatchSize, 2);
    }

    public SentimentManifest Copy()
    {
        return new SentimentManifest(
            Code,
            Label,
            Category,
            ExternalRef,
            Segment,
            Cost,
            BatchSize,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
