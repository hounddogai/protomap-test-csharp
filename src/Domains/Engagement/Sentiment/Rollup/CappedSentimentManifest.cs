namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped sentiment manifest is in its life cycle.</summary>
public enum CappedSentimentManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a capped sentiment manifest and its current status.</summary>
public sealed class CappedSentimentManifest(string code, string title, string currency, string channel, double ceiling, double floorValue, int batchSize, string reviewer, CappedSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public double Ceiling { get; set; } = ceiling;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public CappedSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped sentiment manifest in status proposed.</summary>
    public static CappedSentimentManifest NewCappedSentimentManifest(string code, string title)
    {
        return new CappedSentimentManifest(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            CappedSentimentManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSentimentManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSentimentManifestStatus next)
    {
        switch (Status)
        {
            case CappedSentimentManifestStatus.Proposed:
                switch (next)
                {
                    case CappedSentimentManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedSentimentManifestStatus.Approved:
                return next == CappedSentimentManifestStatus.Rejected || next == CappedSentimentManifestStatus.Withdrawn;
            case CappedSentimentManifestStatus.Rejected:
                switch (next)
                {
                    case CappedSentimentManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped sentiment manifest"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(FloorValue + (double)BatchSize, 2);
    }

    public CappedSentimentManifest Copy()
    {
        return new CappedSentimentManifest(
            Code,
            Title,
            Currency,
            Channel,
            Ceiling,
            FloorValue,
            BatchSize,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
