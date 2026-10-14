namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted bonus pool score calculation.</summary>
public interface IWeightedBonusPoolScoreModifier
{
    string Describe();
    double Adjust(WeightedBonusPoolScore item, double valueValue);
}

public sealed record CapWeightedBonusPoolScoreAdjuster(double Cap) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedBonusPoolScoreAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
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

public sealed record ChannelWeightedBonusPoolScoreAdjuster(Dictionary<string, double> Factors) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedBonusPoolScoreAdjuster(double Minimum) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedBonusPoolScoreAdjuster(double Percent) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWeightedBonusPoolScoreAdjuster(Registry Flags, string Flag, double Percent) : IWeightedBonusPoolScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedBonusPoolScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted bonus pool score steps in order and keeps a trace of each.</summary>
public sealed class WeightedBonusPoolScoreEngine(List<IWeightedBonusPoolScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBonusPoolScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBonusPoolScoreEngine DefaultWeightedBonusPoolScoreEngine(Registry flags)
    {
        return new WeightedBonusPoolScoreEngine(
            new List<IWeightedBonusPoolScoreModifier>
            {
                new CapWeightedBonusPoolScoreAdjuster(20459.0),
                new TieredWeightedBonusPoolScoreAdjuster(
                    new List<double> { 464.0, 596.0, 932.0 },
                    new List<double> { 0.91, 0.99, 1.17 }),
                new ChannelWeightedBonusPoolScoreAdjuster(new Dictionary<string, double> { ["north"] = 1.24 }),
                new MinimumWeightedBonusPoolScoreAdjuster(23.5),
                new PercentWeightedBonusPoolScoreAdjuster(-3.2),
                new FlaggedWeightedBonusPoolScoreAdjuster(flags, "weighted-bonus-pool-score-capped", -7.4),
            },
            new List<string>(),
            1.0,
            40328.7);
    }

    public double Run(WeightedBonusPoolScore item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBonusPoolScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
