namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered sentiment manifest is in its life cycle.</summary>
public enum TieredSentimentManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered sentiment manifest and its current status.</summary>
public sealed class TieredSentimentManifest(string code, string name, string ownerId, int tenureMonths, string locale, double rate, List<string> tags, int expiresAt, TieredSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Locale { get; set; } = locale;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public TieredSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered sentiment manifest in status new.</summary>
    public static TieredSentimentManifest NewTieredSentimentManifest(string code, string name)
    {
        return new TieredSentimentManifest(
            code,
            name,
            "",
            0,
            "",
            0.0,
            new List<string>(),
            0,
            TieredSentimentManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSentimentManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSentimentManifestStatus next)
    {
        switch (Status)
        {
            case TieredSentimentManifestStatus.New:
                switch (next)
                {
                    case TieredSentimentManifestStatus.Queued:
                    case TieredSentimentManifestStatus.Done:
                        return true;
                }
                return false;
            case TieredSentimentManifestStatus.Queued:
                switch (next)
                {
                    case TieredSentimentManifestStatus.Running:
                        return true;
                }
                return false;
            case TieredSentimentManifestStatus.Running:
                return next == TieredSentimentManifestStatus.Done || next == TieredSentimentManifestStatus.Failed;
            case TieredSentimentManifestStatus.Done:
                switch (next)
                {
                    case TieredSentimentManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths * (double)ExpiresAt, 2);
    }

    public TieredSentimentManifest Copy()
    {
        return new TieredSentimentManifest(Code, Name, OwnerId, TenureMonths, Locale, Rate, Tags, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
