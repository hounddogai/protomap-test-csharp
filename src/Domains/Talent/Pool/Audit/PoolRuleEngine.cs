namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pool rule calculation.</summary>
public interface IPoolRuleModifier
{
    string Describe();
    double Adjust(PoolRule item, double valueValue);
}

public sealed record TieredPoolRuleAdjuster(List<double> Bounds, List<double> Rates) : IPoolRuleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolRule item, double valueValue)
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

public sealed record PercentPoolRuleAdjuster(double Percent) : IPoolRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPoolRuleAdjuster(double Cap) : IPoolRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPoolRuleAdjuster(Registry Flags, string Flag, double Percent) : IPoolRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pool rule steps in order and keeps a trace of each.</summary>
public sealed class PoolRuleEngine(List<IPoolRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolRuleEngine DefaultPoolRuleEngine(Registry flags)
    {
        return new PoolRuleEngine(
            new List<IPoolRuleModifier>
            {
                new TieredPoolRuleAdjuster(new List<double> { 634.0, 9277.0 }, new List<double> { 0.82, 0.82 }),
                new PercentPoolRuleAdjuster(-11.4),
                new CapPoolRuleAdjuster(24107.0),
                new FlaggedPoolRuleAdjuster(flags, "pool-rule-default", 2.1),
            },
            new List<string>(),
            4.6,
            77188.3);
    }

    public double Compute(PoolRule item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
