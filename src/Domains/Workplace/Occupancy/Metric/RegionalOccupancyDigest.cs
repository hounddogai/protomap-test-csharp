namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional occupancy digest is in its life cycle.</summary>
public enum RegionalOccupancyDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One regional occupancy digest, with what the planners need to act on it.</summary>
public sealed class RegionalOccupancyDigest(string code, string title, string channel, bool archived, double weight, int priority, int sequence, string badge, Dictionary<string, string> attributes, List<string> tags, RegionalOccupancyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public RegionalOccupancyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a regional occupancy digest in status open.</summary>
    public static RegionalOccupancyDigest NewRegionalOccupancyDigest(string code, string title)
    {
        return new RegionalOccupancyDigest(
            code,
            title,
            "",
            false,
            0.0,
            0,
            0,
            "",
            new Dictionary<string, string>(),
            new List<string>(),
            RegionalOccupancyDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalOccupancyDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalOccupancyDigestStatus next)
    {
        switch (Status)
        {
            case RegionalOccupancyDigestStatus.Open:
                switch (next)
                {
                    case RegionalOccupancyDigestStatus.Resolved:
                    case RegionalOccupancyDigestStatus.Archived:
                        return true;
                }
                return false;
            case RegionalOccupancyDigestStatus.InReview:
                return next == RegionalOccupancyDigestStatus.Resolved || next == RegionalOccupancyDigestStatus.Archived;
            case RegionalOccupancyDigestStatus.Resolved:
                return next == RegionalOccupancyDigestStatus.Reopened || next == RegionalOccupancyDigestStatus.Archived;
            case RegionalOccupancyDigestStatus.Reopened:
                return next == RegionalOccupancyDigestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalOccupancyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional occupancy digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional occupancy digest"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence - Weight, 2);
    }

    public RegionalOccupancyDigest Copy()
    {
        return new RegionalOccupancyDigest(
            Code,
            Title,
            Channel,
            Archived,
            Weight,
            Priority,
            Sequence,
            Badge,
            Attributes,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
