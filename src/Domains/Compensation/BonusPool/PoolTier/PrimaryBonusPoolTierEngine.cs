namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary bonus pool tier calculation.</summary>
public interface IPrimaryBonusPoolTierAdjuster
{
    string Describe();
    double Adjust(PrimaryBonusPoolTier item, double valueValue);
}

public sealed record PercentPrimaryBonusPoolTierAdjuster(double Percent) : IPrimaryBonusPoolTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryBonusPoolTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitPrimaryBonusPoolTierAdjuster(Dictionary<string, double> Factors) : IPrimaryBonusPoolTierAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PrimaryBonusPoolTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryBonusPoolTierLimitOffsetAdjuster(double Weight) : IPrimaryBonusPoolTierAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PrimaryBonusPoolTier item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredPrimaryBonusPoolTierAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryBonusPoolTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryBonusPoolTier item, double valueValue)
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

public sealed record MinimumPrimaryBonusPoolTierAdjuster(double Minimum) : IPrimaryBonusPoolTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryBonusPoolTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary bonus pool tier steps in order and keeps a trace of each.</summary>
public sealed class PrimaryBonusPoolTierEngine(List<IPrimaryBonusPoolTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryBonusPoolTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryBonusPoolTierEngine DefaultPrimaryBonusPoolTierEngine()
    {
        return new PrimaryBonusPoolTierEngine(
            new List<IPrimaryBonusPoolTierAdjuster>
            {
                new PercentPrimaryBonusPoolTierAdjuster(-6.0),
                new UnitPrimaryBonusPoolTierAdjuster(new Dictionary<string, double> { ["central"] = 0.73 }),
                new PrimaryBonusPoolTierLimitOffsetAdjuster(0.92),
                new TieredPrimaryBonusPoolTierAdjuster(
                    new List<double> { 2947.0, 9693.0 },
                    new List<double> { 1.12, 0.95 }),
                new MinimumPrimaryBonusPoolTierAdjuster(42.4),
            },
            new List<string>(),
            1.4,
            92923.4);
    }

    public double Evaluate(PrimaryBonusPoolTier item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryBonusPoolTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
