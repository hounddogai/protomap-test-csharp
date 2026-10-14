namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback exception cycle calculation.</summary>
public interface IFallbackExceptionCycleStep
{
    string Describe();
    double Adjust(FallbackExceptionCycle item, double valueValue);
}

public sealed record CapFallbackExceptionCycleAdjuster(double Cap) : IFallbackExceptionCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackExceptionCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackExceptionCycleAdjuster(List<double> Bounds, List<double> Rates) : IFallbackExceptionCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackExceptionCycle item, double valueValue)
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

public sealed record MinimumFallbackExceptionCycleAdjuster(double Minimum) : IFallbackExceptionCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackExceptionCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackExceptionCycleAdjuster(double Percent) : IFallbackExceptionCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackExceptionCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleFallbackExceptionCycleAdjuster(Dictionary<string, double> Factors) : IFallbackExceptionCycleStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FallbackExceptionCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback exception cycle steps in order and keeps a trace of each.</summary>
public sealed class FallbackExceptionCycleEngine(List<IFallbackExceptionCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackExceptionCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackExceptionCycleEngine DefaultFallbackExceptionCycleEngine()
    {
        return new FallbackExceptionCycleEngine(
            new List<IFallbackExceptionCycleStep>
            {
                new CapFallbackExceptionCycleAdjuster(7454.0),
                new TieredFallbackExceptionCycleAdjuster(
                    new List<double> { 4490.0, 4754.0, 8006.0, 8599.0 },
                    new List<double> { 0.84, 1.01, 1.07, 0.87 }),
                new MinimumFallbackExceptionCycleAdjuster(10.9),
                new PercentFallbackExceptionCycleAdjuster(-3.3),
                new LocaleFallbackExceptionCycleAdjuster(new Dictionary<string, double> { ["online"] = 0.87 }),
            },
            new List<string>(),
            3.1,
            78155.4);
    }

    public double Run(FallbackExceptionCycle item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackExceptionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
