namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback occupancy preference calculation.</summary>
public interface IFallbackOccupancyPreferenceStep
{
    string Describe();
    double Adjust(FallbackOccupancyPreference item, double valueValue);
}

public sealed record RoundingFallbackOccupancyPreferenceAdjuster(double Step) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackOccupancyPreferenceAdjuster(double Percent) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackOccupancyPreferenceRetriesOffsetAdjuster(double Weight) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredFallbackOccupancyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
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

public sealed record MinimumFallbackOccupancyPreferenceAdjuster(double Minimum) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackOccupancyPreferenceAdjuster(double Cap) : IFallbackOccupancyPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackOccupancyPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback occupancy preference steps in order and keeps a trace of each.</summary>
public sealed class FallbackOccupancyPreferenceEngine(List<IFallbackOccupancyPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackOccupancyPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackOccupancyPreferenceEngine DefaultFallbackOccupancyPreferenceEngine()
    {
        return new FallbackOccupancyPreferenceEngine(
            new List<IFallbackOccupancyPreferenceStep>
            {
                new RoundingFallbackOccupancyPreferenceAdjuster(5.0),
                new PercentFallbackOccupancyPreferenceAdjuster(18.2),
                new FallbackOccupancyPreferenceRetriesOffsetAdjuster(1.29),
                new TieredFallbackOccupancyPreferenceAdjuster(
                    new List<double> { 5677.0, 8485.0 },
                    new List<double> { 1.05, 1.11 }),
                new MinimumFallbackOccupancyPreferenceAdjuster(24.9),
                new CapFallbackOccupancyPreferenceAdjuster(41118.0),
            },
            new List<string>(),
            1.6,
            99433.8);
    }

    public double Price(FallbackOccupancyPreference item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackOccupancyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
