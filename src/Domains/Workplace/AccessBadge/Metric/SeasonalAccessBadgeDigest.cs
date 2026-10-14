namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal access badge digest is in its life cycle.</summary>
public enum SeasonalAccessBadgeDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a seasonal access badge digest and its current status.</summary>
public sealed class SeasonalAccessBadgeDigest(string code, string name, string country, Dictionary<string, string> attributes, int sequence, int priority, int version, double hours, string unit, double discount, SeasonalAccessBadgeDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Sequence { get; set; } = sequence;
    public int Priority { get; set; } = priority;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public SeasonalAccessBadgeDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal access badge digest in status idle.</summary>
    public static SeasonalAccessBadgeDigest NewSeasonalAccessBadgeDigest(string code, string name)
    {
        return new SeasonalAccessBadgeDigest(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            0,
            0.0,
            "",
            0.0,
            SeasonalAccessBadgeDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccessBadgeDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccessBadgeDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalAccessBadgeDigestStatus.Idle:
                return next == SeasonalAccessBadgeDigestStatus.Stopped;
            case SeasonalAccessBadgeDigestStatus.Warming:
                return next == SeasonalAccessBadgeDigestStatus.Draining;
            case SeasonalAccessBadgeDigestStatus.Ready:
                return next == SeasonalAccessBadgeDigestStatus.Stopped;
            case SeasonalAccessBadgeDigestStatus.Draining:
                return next == SeasonalAccessBadgeDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalAccessBadgeDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal access badge digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Sequence * Hours, 2);
    }

    public SeasonalAccessBadgeDigest Copy()
    {
        return new SeasonalAccessBadgeDigest(
            Code,
            Name,
            Country,
            Attributes,
            Sequence,
            Priority,
            Version,
            Hours,
            Unit,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
