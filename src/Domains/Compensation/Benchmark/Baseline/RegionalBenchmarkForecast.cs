namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional benchmark forecast is in its life cycle.</summary>
public enum RegionalBenchmarkForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One regional benchmark forecast, with what the planners need to act on it.</summary>
public sealed class RegionalBenchmarkForecast(string code, string name, string team, string badge, int version, bool archived, string label, string segment, double cost, RegionalBenchmarkForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Badge { get; set; } = badge;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public RegionalBenchmarkForecastStatus Status { get; set; } = status;

    /// <summary>Starts a regional benchmark forecast in status idle.</summary>
    public static RegionalBenchmarkForecast NewRegionalBenchmarkForecast(string code, string name)
    {
        return new RegionalBenchmarkForecast(
            code,
            name,
            "",
            "",
            0,
            false,
            "",
            "",
            0.0,
            RegionalBenchmarkForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBenchmarkForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBenchmarkForecastStatus next)
    {
        switch (Status)
        {
            case RegionalBenchmarkForecastStatus.Idle:
                switch (next)
                {
                    case RegionalBenchmarkForecastStatus.Draining:
                    case RegionalBenchmarkForecastStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalBenchmarkForecastStatus.Warming:
                switch (next)
                {
                    case RegionalBenchmarkForecastStatus.Ready:
                    case RegionalBenchmarkForecastStatus.Draining:
                        return true;
                }
                return false;
            case RegionalBenchmarkForecastStatus.Ready:
                switch (next)
                {
                    case RegionalBenchmarkForecastStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalBenchmarkForecastStatus.Draining:
                return next == RegionalBenchmarkForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalBenchmarkForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional benchmark forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version + Cost, 2);
    }

    public RegionalBenchmarkForecast Copy()
    {
        return new RegionalBenchmarkForecast(
            Code,
            Name,
            Team,
            Badge,
            Version,
            Archived,
            Label,
            Segment,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
