namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted sentiment manifest is in its life cycle.</summary>
public enum WeightedSentimentManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a weighted sentiment manifest and its current status.</summary>
public sealed class WeightedSentimentManifest(string code, string title, int version, string department, int limit, bool visible, double margin, string phone, string timezone, WeightedSentimentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Department { get; set; } = department;
    public int Limit { get; set; } = limit;
    public bool Visible { get; set; } = visible;
    public double Margin { get; set; } = margin;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public WeightedSentimentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted sentiment manifest in status created.</summary>
    public static WeightedSentimentManifest NewWeightedSentimentManifest(string code, string title)
    {
        return new WeightedSentimentManifest(
            code,
            title,
            0,
            "",
            0,
            false,
            0.0,
            "",
            "",
            WeightedSentimentManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSentimentManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSentimentManifestStatus next)
    {
        switch (Status)
        {
            case WeightedSentimentManifestStatus.Created:
                return next == WeightedSentimentManifestStatus.Validated;
            case WeightedSentimentManifestStatus.Validated:
                return next == WeightedSentimentManifestStatus.Retired;
            case WeightedSentimentManifestStatus.Published:
                return next == WeightedSentimentManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedSentimentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted sentiment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Limit + Margin, 2);
    }

    public WeightedSentimentManifest Copy()
    {
        return new WeightedSentimentManifest(
            Code,
            Title,
            Version,
            Department,
            Limit,
            Visible,
            Margin,
            Phone,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
