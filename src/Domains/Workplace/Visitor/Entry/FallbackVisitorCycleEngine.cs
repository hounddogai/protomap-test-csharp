namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback visitor cycle calculation.</summary>
public interface IFallbackVisitorCycleAdjuster
{
    string Describe();
    double Adjust(FallbackVisitorCycle item, double valueValue);
}

public sealed record RoundingFallbackVisitorCycleAdjuster(double Step) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFallbackVisitorCycleAdjuster(List<double> Bounds, List<double> Rates) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
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

public sealed record CapFallbackVisitorCycleAdjuster(double Cap) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackVisitorCycleRatingOffsetAdjuster(double Weight) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record MinimumFallbackVisitorCycleAdjuster(double Minimum) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackVisitorCycleAdjuster(double Percent) : IFallbackVisitorCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackVisitorCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback visitor cycle steps in order and keeps a trace of each.</summary>
public sealed class FallbackVisitorCycleEngine(List<IFallbackVisitorCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackVisitorCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackVisitorCycleEngine DefaultFallbackVisitorCycleEngine()
    {
        return new FallbackVisitorCycleEngine(
            new List<IFallbackVisitorCycleAdjuster>
            {
                new RoundingFallbackVisitorCycleAdjuster(1.0),
                new TieredFallbackVisitorCycleAdjuster(
                    new List<double> { 1137.0, 4590.0, 9383.0 },
                    new List<double> { 1.0, 1.03, 0.84 }),
                new CapFallbackVisitorCycleAdjuster(26264.0),
                new FallbackVisitorCycleRatingOffsetAdjuster(0.97),
                new MinimumFallbackVisitorCycleAdjuster(26.1),
                new PercentFallbackVisitorCycleAdjuster(7.6),
            },
            new List<string>(),
            2.0,
            72833.2);
    }

    public double Run(FallbackVisitorCycle item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackVisitorCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
