namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>One step of the fallback refresh policy calculation.</summary>
public interface IFallbackRefreshPolicyAdjuster
{
    string Describe();
    double Adjust(FallbackRefreshPolicy item, double valueValue);
}

public sealed record CapFallbackRefreshPolicyAdjuster(double Cap) : IFallbackRefreshPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackRefreshPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackRefreshPolicyAdjuster(List<double> Bounds, List<double> Rates) : IFallbackRefreshPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackRefreshPolicy item, double valueValue)
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

public sealed record SourceFallbackRefreshPolicyAdjuster(Dictionary<string, double> Factors) : IFallbackRefreshPolicyAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(FallbackRefreshPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback refresh policy steps in order and keeps a trace of each.</summary>
public sealed class FallbackRefreshPolicyEngine(List<IFallbackRefreshPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRefreshPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRefreshPolicyEngine DefaultFallbackRefreshPolicyEngine()
    {
        return new FallbackRefreshPolicyEngine(
            new List<IFallbackRefreshPolicyAdjuster>
            {
                new CapFallbackRefreshPolicyAdjuster(48173.0),
                new TieredFallbackRefreshPolicyAdjuster(
                    new List<double> { 2827.0, 2988.0, 4211.0, 8915.0 },
                    new List<double> { 1.07, 1.19, 0.93, 0.93 }),
                new SourceFallbackRefreshPolicyAdjuster(new Dictionary<string, double> { ["north"] = 0.75 }),
            },
            new List<string>(),
            2.8,
            44537.1);
    }

    public double Run(FallbackRefreshPolicy item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRefreshPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
