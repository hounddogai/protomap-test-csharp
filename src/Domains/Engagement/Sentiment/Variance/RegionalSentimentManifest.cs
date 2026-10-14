namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional sentiment manifest is in its life cycle.</summary>
public enum RegionalSentimentManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a regional sentiment manifest and its current status.</summary>
public sealed class RegionalSentimentManifest(string code, string name, bool visible, string ownerId, int version, double margin, double ratio, int graceDays, RegionalSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public double Margin { get; set; } = margin;
    public double Ratio { get; set; } = ratio;
    public int GraceDays { get; set; } = graceDays;
    public RegionalSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a regional sentiment manifest in status planned.</summary>
    public static RegionalSentimentManifest NewRegionalSentimentManifest(string code, string name)
    {
        return new RegionalSentimentManifest(
            code,
            name,
            false,
            "",
            0,
            0.0,
            0.0,
            0,
            RegionalSentimentManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSentimentManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSentimentManifestStatus next)
    {
        switch (Status)
        {
            case RegionalSentimentManifestStatus.Planned:
                switch (next)
                {
                    case RegionalSentimentManifestStatus.Completed:
                        return true;
                }
                return false;
            case RegionalSentimentManifestStatus.Scheduled:
                switch (next)
                {
                    case RegionalSentimentManifestStatus.Completed:
                        return true;
                }
                return false;
            case RegionalSentimentManifestStatus.InProgress:
                return next == RegionalSentimentManifestStatus.Cancelled;
            case RegionalSentimentManifestStatus.Completed:
                switch (next)
                {
                    case RegionalSentimentManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"regional sentiment manifest"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Version * (double)GraceDays, 2);
    }

    public RegionalSentimentManifest Copy()
    {
        return new RegionalSentimentManifest(
            Code,
            Name,
            Visible,
            OwnerId,
            Version,
            Margin,
            Ratio,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
