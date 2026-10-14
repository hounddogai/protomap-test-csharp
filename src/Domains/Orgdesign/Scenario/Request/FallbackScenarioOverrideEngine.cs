namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback scenario override calculation.</summary>
public interface IFallbackScenarioOverrideModifier
{
    string Describe();
    double Adjust(FallbackScenarioOverride item, double valueValue);
}

public sealed record PercentFallbackScenarioOverrideAdjuster(double Percent) : IFallbackScenarioOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackScenarioOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionFallbackScenarioOverrideAdjuster(Dictionary<string, double> Factors) : IFallbackScenarioOverrideModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FallbackScenarioOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackScenarioOverrideAdjuster(List<double> Bounds, List<double> Rates) : IFallbackScenarioOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackScenarioOverride item, double valueValue)
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

public sealed record CapFallbackScenarioOverrideAdjuster(double Cap) : IFallbackScenarioOverrideModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackScenarioOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback scenario override steps in order and keeps a trace of each.</summary>
public sealed class FallbackScenarioOverrideEngine(List<IFallbackScenarioOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackScenarioOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackScenarioOverrideEngine DefaultFallbackScenarioOverrideEngine()
    {
        return new FallbackScenarioOverrideEngine(
            new List<IFallbackScenarioOverrideModifier>
            {
                new PercentFallbackScenarioOverrideAdjuster(7.1),
                new RegionFallbackScenarioOverrideAdjuster(new Dictionary<string, double> { ["online"] = 1.3 }),
                new TieredFallbackScenarioOverrideAdjuster(
                    new List<double> { 2445.0, 5632.0 },
                    new List<double> { 1.12, 0.85 }),
                new CapFallbackScenarioOverrideAdjuster(25344.0),
            },
            new List<string>(),
            0.1,
            90011.6);
    }

    public double Evaluate(FallbackScenarioOverride item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackScenarioOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
