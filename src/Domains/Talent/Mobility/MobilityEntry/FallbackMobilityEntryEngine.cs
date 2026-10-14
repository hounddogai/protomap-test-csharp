namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>One step of the fallback mobility entry calculation.</summary>
public interface IFallbackMobilityEntryModifier
{
    string Describe();
    double Adjust(FallbackMobilityEntry item, double valueValue);
}

public sealed record MinimumFallbackMobilityEntryAdjuster(double Minimum) : IFallbackMobilityEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackMobilityEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackMobilityEntryVersionOffsetAdjuster(double Weight) : IFallbackMobilityEntryModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(FallbackMobilityEntry item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredFallbackMobilityEntryAdjuster(List<double> Bounds, List<double> Rates) : IFallbackMobilityEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackMobilityEntry item, double valueValue)
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

/// <summary>Runs the fallback mobility entry steps in order and keeps a trace of each.</summary>
public sealed class FallbackMobilityEntryEngine(List<IFallbackMobilityEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackMobilityEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackMobilityEntryEngine DefaultFallbackMobilityEntryEngine()
    {
        return new FallbackMobilityEntryEngine(
            new List<IFallbackMobilityEntryModifier>
            {
                new MinimumFallbackMobilityEntryAdjuster(43.9),
                new FallbackMobilityEntryVersionOffsetAdjuster(0.21),
                new TieredFallbackMobilityEntryAdjuster(
                    new List<double> { 1913.0, 2922.0, 7529.0 },
                    new List<double> { 0.9, 0.96, 0.88 }),
            },
            new List<string>(),
            3.7,
            7554.6);
    }

    public double Compute(FallbackMobilityEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackMobilityEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
