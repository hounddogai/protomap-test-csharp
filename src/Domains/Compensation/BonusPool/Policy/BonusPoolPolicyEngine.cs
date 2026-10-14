namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the bonus pool policy calculation.</summary>
public interface IBonusPoolPolicyFactor
{
    string Describe();
    double Adjust(BonusPoolPolicy item, double valueValue);
}

public sealed record PercentBonusPoolPolicyAdjuster(double Percent) : IBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBonusPoolPolicyAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolPolicyRatioOffsetAdjuster(double Weight) : IBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(BonusPoolPolicy item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record NameBonusPoolPolicyAdjuster(Dictionary<string, double> Factors) : IBonusPoolPolicyFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BonusPoolPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBonusPoolPolicyAdjuster(double Minimum) : IBonusPoolPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the bonus pool policy steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolPolicyEngine(List<IBonusPoolPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolPolicyEngine DefaultBonusPoolPolicyEngine(Registry flags)
    {
        return new BonusPoolPolicyEngine(
            new List<IBonusPoolPolicyFactor>
            {
                new PercentBonusPoolPolicyAdjuster(12.9),
                new FlaggedBonusPoolPolicyAdjuster(flags, "bonus-pool-policy-primary", -6.5),
                new BonusPoolPolicyRatioOffsetAdjuster(1.41),
                new NameBonusPoolPolicyAdjuster(new Dictionary<string, double> { ["central"] = 0.74 }),
                new MinimumBonusPoolPolicyAdjuster(43.1),
            },
            new List<string>(),
            1.6,
            40612.3);
    }

    public double Compute(BonusPoolPolicy item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
