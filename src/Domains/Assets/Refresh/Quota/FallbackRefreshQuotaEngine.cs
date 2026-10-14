namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback refresh quota calculation.</summary>
public interface IFallbackRefreshQuotaFactor
{
    string Describe();
    double Adjust(FallbackRefreshQuota item, double valueValue);
}

public sealed record MinimumFallbackRefreshQuotaAdjuster(double Minimum) : IFallbackRefreshQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRefreshQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleFallbackRefreshQuotaAdjuster(Dictionary<string, double> Factors) : IFallbackRefreshQuotaFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FallbackRefreshQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackRefreshQuotaAdjuster(Registry Flags, string Flag, double Percent) : IFallbackRefreshQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackRefreshQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackRefreshQuotaAdjuster(double Percent) : IFallbackRefreshQuotaFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRefreshQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback refresh quota steps in order and keeps a trace of each.</summary>
public sealed class FallbackRefreshQuotaEngine(List<IFallbackRefreshQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRefreshQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRefreshQuotaEngine DefaultFallbackRefreshQuotaEngine(Registry flags)
    {
        return new FallbackRefreshQuotaEngine(
            new List<IFallbackRefreshQuotaFactor>
            {
                new MinimumFallbackRefreshQuotaAdjuster(2.3),
                new LocaleFallbackRefreshQuotaAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.95, ["online"] = 1.08 }),
                new FlaggedFallbackRefreshQuotaAdjuster(flags, "fallback-refresh-quota-global", 1.1),
                new PercentFallbackRefreshQuotaAdjuster(8.3),
            },
            new List<string>(),
            1.2,
            83516.2);
    }

    public double Evaluate(FallbackRefreshQuota item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackRefreshQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
