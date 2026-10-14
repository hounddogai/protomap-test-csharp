namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling refresh forecast calculation.</summary>
public interface IRollingRefreshForecastModifier
{
    string Describe();
    double Adjust(RollingRefreshForecast item, double valueValue);
}

public sealed record TieredRollingRefreshForecastAdjuster(List<double> Bounds, List<double> Rates) : IRollingRefreshForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingRefreshForecast item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record RollingRefreshForecastExpiresAtOffsetAdjuster(double Weight) : IRollingRefreshForecastModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RollingRefreshForecast item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentRollingRefreshForecastAdjuster(double Percent) : IRollingRefreshForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingRefreshForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingRefreshForecastAdjuster(Registry Flags, string Flag, double Percent) : IRollingRefreshForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingRefreshForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling refresh forecast steps in order and keeps a trace of each.</summary>
public sealed class RollingRefreshForecastEngine(List<IRollingRefreshForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRefreshForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRefreshForecastEngine DefaultRollingRefreshForecastEngine(Registry flags)
    {
        return new RollingRefreshForecastEngine(
            new List<IRollingRefreshForecastModifier>
            {
                new TieredRollingRefreshForecastAdjuster(
                    new List<double> { 3029.0, 9402.0 },
                    new List<double> { 1.04, 1.05 }),
                new RollingRefreshForecastExpiresAtOffsetAdjuster(2.0),
                new PercentRollingRefreshForecastAdjuster(-7.2),
                new FlaggedRollingRefreshForecastAdjuster(flags, "rolling-refresh-forecast-pending", 9.1),
            },
            new List<string>(),
            0.8,
            32872.8);
    }

    public double Price(RollingRefreshForecast item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRefreshForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
