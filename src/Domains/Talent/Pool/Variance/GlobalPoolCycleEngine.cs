namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pool cycle calculation.</summary>
public interface IGlobalPoolCycleFactor
{
    string Describe();
    double Adjust(GlobalPoolCycle item, double valueValue);
}

public sealed record MinimumGlobalPoolCycleAdjuster(double Minimum) : IGlobalPoolCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPoolCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalPoolCycleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPoolCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPoolCycle item, double valueValue)
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

public sealed record GlobalPoolCycleRatingOffsetAdjuster(double Weight) : IGlobalPoolCycleFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(GlobalPoolCycle item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record RoundingGlobalPoolCycleAdjuster(double Step) : IGlobalPoolCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPoolCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global pool cycle steps in order and keeps a trace of each.</summary>
public sealed class GlobalPoolCycleEngine(List<IGlobalPoolCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPoolCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPoolCycleEngine DefaultGlobalPoolCycleEngine()
    {
        return new GlobalPoolCycleEngine(
            new List<IGlobalPoolCycleFactor>
            {
                new MinimumGlobalPoolCycleAdjuster(28.8),
                new TieredGlobalPoolCycleAdjuster(
                    new List<double> { 2231.0, 6841.0, 8388.0, 8851.0 },
                    new List<double> { 1.05, 0.95, 1.07, 0.82 }),
                new GlobalPoolCycleRatingOffsetAdjuster(1.87),
                new RoundingGlobalPoolCycleAdjuster(0.5),
            },
            new List<string>(),
            1.4,
            66309.7);
    }

    public double Run(GlobalPoolCycle item)
    {
        var current = item.ProjectedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<GlobalPoolCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
