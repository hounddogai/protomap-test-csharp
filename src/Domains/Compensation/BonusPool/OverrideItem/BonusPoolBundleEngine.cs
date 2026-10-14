namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool bundle calculation.</summary>
public interface IBonusPoolBundleStep
{
    string Describe();
    double Adjust(BonusPoolBundle item, double valueValue);
}

public sealed record CapBonusPoolBundleAdjuster(double Cap) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBonusPoolBundleAdjuster(double Step) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBonusPoolBundleAdjuster(double Percent) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBonusPoolBundleAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BonusPoolBundleRatioOffsetAdjuster(double Weight) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record LabelBonusPoolBundleAdjuster(Dictionary<string, double> Factors) : IBonusPoolBundleStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BonusPoolBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool bundle steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolBundleEngine(List<IBonusPoolBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolBundleEngine DefaultBonusPoolBundleEngine(Registry flags)
    {
        return new BonusPoolBundleEngine(
            new List<IBonusPoolBundleStep>
            {
                new CapBonusPoolBundleAdjuster(40625.0),
                new RoundingBonusPoolBundleAdjuster(0.25),
                new PercentBonusPoolBundleAdjuster(-17.1),
                new FlaggedBonusPoolBundleAdjuster(flags, "bonus-pool-bundle-seasonal", 1.2),
                new BonusPoolBundleRatioOffsetAdjuster(0.15),
                new LabelBonusPoolBundleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.85, ["west"] = 1.38, ["south"] = 1.23 }),
            },
            new List<string>(),
            3.9,
            71497.5);
    }

    public double Price(BonusPoolBundle item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
