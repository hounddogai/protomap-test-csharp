namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool baseline calculation.</summary>
public interface IPoolBaselineModifier
{
    string Describe();
    double Adjust(PoolBaseline item, double valueValue);
}

public sealed record RoundingPoolBaselineAdjuster(double Step) : IPoolBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPoolBaselineAdjuster(double Minimum) : IPoolBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPoolBaselineAdjuster(double Percent) : IPoolBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPoolBaselineAdjuster(List<double> Bounds, List<double> Rates) : IPoolBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolBaseline item, double valueValue)
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

public sealed record NamePoolBaselineAdjuster(Dictionary<string, double> Factors) : IPoolBaselineModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PoolBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pool baseline steps in order and keeps a trace of each.</summary>
public sealed class PoolBaselineEngine(List<IPoolBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolBaselineEngine DefaultPoolBaselineEngine()
    {
        return new PoolBaselineEngine(
            new List<IPoolBaselineModifier>
            {
                new RoundingPoolBaselineAdjuster(5.0),
                new MinimumPoolBaselineAdjuster(16.1),
                new PercentPoolBaselineAdjuster(22.3),
                new TieredPoolBaselineAdjuster(new List<double> { 780.0, 7535.0 }, new List<double> { 1.14, 0.82 }),
                new NamePoolBaselineAdjuster(new Dictionary<string, double> { ["east"] = 0.86 }),
            },
            new List<string>(),
            1.7,
            12581.1);
    }

    public double Evaluate(PoolBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
