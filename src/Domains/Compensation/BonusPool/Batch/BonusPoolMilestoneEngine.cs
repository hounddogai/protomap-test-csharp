namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool milestone calculation.</summary>
public interface IBonusPoolMilestoneFactor
{
    string Describe();
    double Adjust(BonusPoolMilestone item, double valueValue);
}

public sealed record RoundingBonusPoolMilestoneAdjuster(double Step) : IBonusPoolMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBonusPoolMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolMilestone item, double valueValue)
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

public sealed record MinimumBonusPoolMilestoneAdjuster(double Minimum) : IBonusPoolMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleBonusPoolMilestoneAdjuster(Dictionary<string, double> Factors) : IBonusPoolMilestoneFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BonusPoolMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBonusPoolMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool milestone steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolMilestoneEngine(List<IBonusPoolMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolMilestoneEngine DefaultBonusPoolMilestoneEngine(Registry flags)
    {
        return new BonusPoolMilestoneEngine(
            new List<IBonusPoolMilestoneFactor>
            {
                new RoundingBonusPoolMilestoneAdjuster(0.1),
                new TieredBonusPoolMilestoneAdjuster(
                    new List<double> { 877.0, 4922.0 },
                    new List<double> { 1.14, 1.1 }),
                new MinimumBonusPoolMilestoneAdjuster(39.1),
                new TitleBonusPoolMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.39, ["north"] = 1.19, ["online"] = 1.05 }),
                new FlaggedBonusPoolMilestoneAdjuster(flags, "bonus-pool-milestone-lenient", 7.3),
            },
            new List<string>(),
            0.9,
            34223.5);
    }

    public double Run(BonusPoolMilestone item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
