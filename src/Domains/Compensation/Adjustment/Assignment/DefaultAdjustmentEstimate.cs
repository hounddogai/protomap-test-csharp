namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default adjustment estimate is in its life cycle.</summary>
public enum DefaultAdjustmentEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default adjustment estimate. Its code is unique within a tenant.</summary>
public sealed class DefaultAdjustmentEstimate(string code, string name, double rate, int durationMinutes, string region, double rating, double minimum, bool approved, string title, string checksum, Dictionary<string, string> attributes, DefaultAdjustmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Region { get; set; } = region;
    public double Rating { get; set; } = rating;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public DefaultAdjustmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a default adjustment estimate in status idle.</summary>
    public static DefaultAdjustmentEstimate NewDefaultAdjustmentEstimate(string code, string name)
    {
        return new DefaultAdjustmentEstimate(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            0.0,
            false,
            "",
            "",
            new Dictionary<string, string>(),
            DefaultAdjustmentEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAdjustmentEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAdjustmentEstimateStatus next)
    {
        switch (Status)
        {
            case DefaultAdjustmentEstimateStatus.Idle:
                return next == DefaultAdjustmentEstimateStatus.Ready;
            case DefaultAdjustmentEstimateStatus.Warming:
                switch (next)
                {
                    case DefaultAdjustmentEstimateStatus.Ready:
                        return true;
                }
                return false;
            case DefaultAdjustmentEstimateStatus.Ready:
                return next == DefaultAdjustmentEstimateStatus.Stopped;
            case DefaultAdjustmentEstimateStatus.Draining:
                return next == DefaultAdjustmentEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultAdjustmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default adjustment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double WeightedVolume()
    {
        return Math.Round(Minimum * Rating, 2);
    }

    public DefaultAdjustmentEstimate Copy()
    {
        return new DefaultAdjustmentEstimate(
            Code,
            Name,
            Rate,
            DurationMinutes,
            Region,
            Rating,
            Minimum,
            Approved,
            Title,
            Checksum,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
