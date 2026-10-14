namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool metric is in its life cycle.</summary>
public enum PoolMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pool metric and its current status.</summary>
public sealed class PoolMetric(string code, string title, double fte, double discount, int createdAt, int version, List<string> tags, string location, string currency, PoolMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public double Discount { get; set; } = discount;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public List<string> Tags { get; set; } = tags;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public PoolMetricStatus Status { get; set; } = status;

    /// <summary>Starts a pool metric in status idle.</summary>
    public static PoolMetric NewPoolMetric(string code, string title)
    {
        return new PoolMetric(code, title, 0.0, 0.0, 0, 0, new List<string>(), "", "", PoolMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolMetricStatus next)
    {
        switch (Status)
        {
            case PoolMetricStatus.Idle:
                return next == PoolMetricStatus.Warming || next == PoolMetricStatus.Ready;
            case PoolMetricStatus.Warming:
                switch (next)
                {
                    case PoolMetricStatus.Ready:
                        return true;
                }
                return false;
            case PoolMetricStatus.Ready:
                switch (next)
                {
                    case PoolMetricStatus.Draining:
                    case PoolMetricStatus.Stopped:
                        return true;
                }
                return false;
            case PoolMetricStatus.Draining:
                return next == PoolMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PoolMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"pool metric"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt + Discount, 2);
    }

    public PoolMetric Copy()
    {
        return new PoolMetric(Code, Title, Fte, Discount, CreatedAt, Version, Tags, Location, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
