namespace Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>One step of the global span entry calculation.</summary>
public interface IGlobalSpanEntryStep
{
    string Describe();
    double Adjust(GlobalSpanEntry item, double valueValue);
}

public sealed record MinimumGlobalSpanEntryAdjuster(double Minimum) : IGlobalSpanEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSpanEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalSpanEntryAdjuster(List<double> Bounds, List<double> Rates) : IGlobalSpanEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalSpanEntry item, double valueValue)
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

public sealed record PercentGlobalSpanEntryAdjuster(double Percent) : IGlobalSpanEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalSpanEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalSpanEntryFloorValueOffsetAdjuster(double Weight) : IGlobalSpanEntryStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(GlobalSpanEntry item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record RegionGlobalSpanEntryAdjuster(Dictionary<string, double> Factors) : IGlobalSpanEntryStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(GlobalSpanEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global span entry steps in order and keeps a trace of each.</summary>
public sealed class GlobalSpanEntryEngine(List<IGlobalSpanEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSpanEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSpanEntryEngine DefaultGlobalSpanEntryEngine()
    {
        return new GlobalSpanEntryEngine(
            new List<IGlobalSpanEntryStep>
            {
                new MinimumGlobalSpanEntryAdjuster(9.7),
                new TieredGlobalSpanEntryAdjuster(
                    new List<double> { 602.0, 927.0, 3944.0 },
                    new List<double> { 0.8, 1.17, 0.91 }),
                new PercentGlobalSpanEntryAdjuster(-8.0),
                new GlobalSpanEntryFloorValueOffsetAdjuster(1.69),
                new RegionGlobalSpanEntryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.03, ["online"] = 1.01, ["west"] = 0.92 }),
            },
            new List<string>(),
            1.4,
            23818.4);
    }

    public double Compute(GlobalSpanEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSpanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
