namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool plan calculation.</summary>
public interface IBonusPoolPlanStep
{
    string Describe();
    double Adjust(BonusPoolPlan item, double valueValue);
}

public sealed record TieredBonusPoolPlanAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolPlan item, double valueValue)
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

public sealed record PercentBonusPoolPlanAdjuster(double Percent) : IBonusPoolPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolPlanRatingOffsetAdjuster(double Weight) : IBonusPoolPlanStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(BonusPoolPlan item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record FlaggedBonusPoolPlanAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBonusPoolPlanAdjuster(double Minimum) : IBonusPoolPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the bonus pool plan steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolPlanEngine(List<IBonusPoolPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolPlanEngine DefaultBonusPoolPlanEngine(Registry flags)
    {
        return new BonusPoolPlanEngine(
            new List<IBonusPoolPlanStep>
            {
                new TieredBonusPoolPlanAdjuster(
                    new List<double> { 2045.0, 6409.0, 8271.0, 8552.0 },
                    new List<double> { 0.85, 0.81, 1.08, 0.98 }),
                new PercentBonusPoolPlanAdjuster(-17.2),
                new BonusPoolPlanRatingOffsetAdjuster(1.39),
                new FlaggedBonusPoolPlanAdjuster(flags, "bonus-pool-plan-tiered", -8.4),
                new MinimumBonusPoolPlanAdjuster(31.9),
            },
            new List<string>(),
            3.1,
            10597.3);
    }

    public double Run(BonusPoolPlan item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
