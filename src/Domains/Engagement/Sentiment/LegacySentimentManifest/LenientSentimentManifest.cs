namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient sentiment manifest is in its life cycle.</summary>
public enum LenientSentimentManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient sentiment manifest as the sentiment team keeps it.</summary>
public sealed class LenientSentimentManifest(string code, string title, bool approved, int graceDays, string category, int version, int quantity, int durationMinutes, int endsAt, string badge, string managerId, LenientSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public string Badge { get; set; } = badge;
    public string ManagerId { get; set; } = managerId;
    public LenientSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient sentiment manifest in status idle.</summary>
    public static LenientSentimentManifest NewLenientSentimentManifest(string code, string title)
    {
        return new LenientSentimentManifest(
            code,
            title,
            false,
            0,
            "",
            0,
            0,
            0,
            0,
            "",
            "",
            LenientSentimentManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSentimentManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSentimentManifestStatus next)
    {
        switch (Status)
        {
            case LenientSentimentManifestStatus.Idle:
                return next == LenientSentimentManifestStatus.Ready || next == LenientSentimentManifestStatus.Draining;
            case LenientSentimentManifestStatus.Warming:
                switch (next)
                {
                    case LenientSentimentManifestStatus.Ready:
                    case LenientSentimentManifestStatus.Draining:
                        return true;
                }
                return false;
            case LenientSentimentManifestStatus.Ready:
                switch (next)
                {
                    case LenientSentimentManifestStatus.Draining:
                    case LenientSentimentManifestStatus.Stopped:
                        return true;
                }
                return false;
            case LenientSentimentManifestStatus.Draining:
                switch (next)
                {
                    case LenientSentimentManifestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)EndsAt + (double)GraceDays, 2);
    }

    public LenientSentimentManifest Copy()
    {
        return new LenientSentimentManifest(
            Code,
            Title,
            Approved,
            GraceDays,
            Category,
            Version,
            Quantity,
            DurationMinutes,
            EndsAt,
            Badge,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
