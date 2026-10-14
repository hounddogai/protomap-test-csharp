namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling sentiment summary is in its life cycle.</summary>
public enum RollingSentimentSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling sentiment summary. Its code is unique within a tenant.</summary>
public sealed class RollingSentimentSummary(string code, string name, int version, string source, int batchSize, int leaveDays, double floorValue, double ceiling, int expiresAt, double weight, RollingSentimentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Weight { get; set; } = weight;
    public RollingSentimentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling sentiment summary in status open.</summary>
    public static RollingSentimentSummary NewRollingSentimentSummary(string code, string name)
    {
        return new RollingSentimentSummary(
            code,
            name,
            0,
            "",
            0,
            0,
            0.0,
            0.0,
            0,
            0.0,
            RollingSentimentSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSentimentSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSentimentSummaryStatus next)
    {
        switch (Status)
        {
            case RollingSentimentSummaryStatus.Open:
                return next == RollingSentimentSummaryStatus.Resolved;
            case RollingSentimentSummaryStatus.InReview:
                switch (next)
                {
                    case RollingSentimentSummaryStatus.Resolved:
                        return true;
                }
                return false;
            case RollingSentimentSummaryStatus.Resolved:
                return next == RollingSentimentSummaryStatus.Reopened || next == RollingSentimentSummaryStatus.Archived;
            case RollingSentimentSummaryStatus.Reopened:
                return next == RollingSentimentSummaryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingSentimentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling sentiment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling * (double)BatchSize, 2);
    }

    public RollingSentimentSummary Copy()
    {
        return new RollingSentimentSummary(
            Code,
            Name,
            Version,
            Source,
            BatchSize,
            LeaveDays,
            FloorValue,
            Ceiling,
            ExpiresAt,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
