namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling equity grant metric is in its life cycle.</summary>
public enum RollingEquityGrantMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rolling equity grant metric, with what the planners need to act on it.</summary>
public sealed class RollingEquityGrantMetric(string code, string title, string name, double weight, int limit, double rate, string timezone, string source, int createdAt, double cost, RollingEquityGrantMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public int CreatedAt { get; set; } = createdAt;
    public double Cost { get; set; } = cost;
    public RollingEquityGrantMetricStatus Status { get; set; } = status;

    /// <summary>Starts a rolling equity grant metric in status draft.</summary>
    public static RollingEquityGrantMetric NewRollingEquityGrantMetric(string code, string title)
    {
        return new RollingEquityGrantMetric(
            code,
            title,
            "",
            0.0,
            0,
            0.0,
            "",
            "",
            0,
            0.0,
            RollingEquityGrantMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingEquityGrantMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingEquityGrantMetricStatus next)
    {
        switch (Status)
        {
            case RollingEquityGrantMetricStatus.Draft:
                switch (next)
                {
                    case RollingEquityGrantMetricStatus.Closed:
                        return true;
                }
                return false;
            case RollingEquityGrantMetricStatus.Pending:
                return next == RollingEquityGrantMetricStatus.Active || next == RollingEquityGrantMetricStatus.Suspended;
            case RollingEquityGrantMetricStatus.Active:
                return next == RollingEquityGrantMetricStatus.Closed;
            case RollingEquityGrantMetricStatus.Suspended:
                switch (next)
                {
                    case RollingEquityGrantMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingEquityGrantMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling equity grant metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit - Weight, 2);
    }

    public RollingEquityGrantMetric Copy()
    {
        return new RollingEquityGrantMetric(
            Code,
            Title,
            Name,
            Weight,
            Limit,
            Rate,
            Timezone,
            Source,
            CreatedAt,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
