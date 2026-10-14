namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling bonus pool template calculation.</summary>
public interface IRollingBonusPoolTemplateAdjuster
{
    string Describe();
    double Adjust(RollingBonusPoolTemplate item, double valueValue);
}

public sealed record UnitRollingBonusPoolTemplateAdjuster(Dictionary<string, double> Factors) : IRollingBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RollingBonusPoolTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingBonusPoolTemplateWeightOffsetAdjuster(double Weight) : IRollingBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RollingBonusPoolTemplate item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record TieredRollingBonusPoolTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRollingBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingBonusPoolTemplate item, double valueValue)
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

public sealed record MinimumRollingBonusPoolTemplateAdjuster(double Minimum) : IRollingBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingBonusPoolTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRollingBonusPoolTemplateAdjuster(Registry Flags, string Flag, double Percent) : IRollingBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBonusPoolTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling bonus pool template steps in order and keeps a trace of each.</summary>
public sealed class RollingBonusPoolTemplateEngine(List<IRollingBonusPoolTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBonusPoolTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBonusPoolTemplateEngine DefaultRollingBonusPoolTemplateEngine(Registry flags)
    {
        return new RollingBonusPoolTemplateEngine(
            new List<IRollingBonusPoolTemplateAdjuster>
            {
                new UnitRollingBonusPoolTemplateAdjuster(new Dictionary<string, double> { ["retail"] = 0.91 }),
                new RollingBonusPoolTemplateWeightOffsetAdjuster(0.58),
                new TieredRollingBonusPoolTemplateAdjuster(
                    new List<double> { 1941.0, 9860.0 },
                    new List<double> { 1.13, 1.13 }),
                new MinimumRollingBonusPoolTemplateAdjuster(41.4),
                new FlaggedRollingBonusPoolTemplateAdjuster(flags, "rolling-bonus-pool-template-legacy", 2.2),
            },
            new List<string>(),
            0.5,
            95000.8);
    }

    public double Compute(RollingBonusPoolTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBonusPoolTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
