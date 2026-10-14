namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool bundle calculation.</summary>
public interface IPoolBundleModifier
{
    string Describe();
    double Adjust(PoolBundle item, double valueValue);
}

public sealed record CapPoolBundleAdjuster(double Cap) : IPoolBundleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPoolBundleAdjuster(List<double> Bounds, List<double> Rates) : IPoolBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolBundle item, double valueValue)
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

public sealed record DepartmentPoolBundleAdjuster(Dictionary<string, double> Factors) : IPoolBundleModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PoolBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pool bundle steps in order and keeps a trace of each.</summary>
public sealed class PoolBundleEngine(List<IPoolBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolBundleEngine DefaultPoolBundleEngine()
    {
        return new PoolBundleEngine(
            new List<IPoolBundleModifier>
            {
                new CapPoolBundleAdjuster(16071.0),
                new TieredPoolBundleAdjuster(new List<double> { 5385.0, 6940.0 }, new List<double> { 1.03, 0.98 }),
                new DepartmentPoolBundleAdjuster(new Dictionary<string, double> { ["west"] = 1.32 }),
            },
            new List<string>(),
            3.0,
            59751.3);
    }

    public double Evaluate(PoolBundle item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
