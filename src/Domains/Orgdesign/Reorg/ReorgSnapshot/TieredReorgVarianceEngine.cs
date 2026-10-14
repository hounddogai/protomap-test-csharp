namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered reorg variance calculation.</summary>
public interface ITieredReorgVarianceStep
{
    string Describe();
    double Adjust(TieredReorgVariance item, double valueValue);
}

public sealed record FlaggedTieredReorgVarianceAdjuster(Registry Flags, string Flag, double Percent) : ITieredReorgVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredReorgVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredReorgVarianceAdjuster(double Cap) : ITieredReorgVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredReorgVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentTieredReorgVarianceAdjuster(Dictionary<string, double> Factors) : ITieredReorgVarianceStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredReorgVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered reorg variance steps in order and keeps a trace of each.</summary>
public sealed class TieredReorgVarianceEngine(List<ITieredReorgVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredReorgVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredReorgVarianceEngine DefaultTieredReorgVarianceEngine(Registry flags)
    {
        return new TieredReorgVarianceEngine(
            new List<ITieredReorgVarianceStep>
            {
                new FlaggedTieredReorgVarianceAdjuster(flags, "tiered-reorg-variance-capped", -4.0),
                new CapTieredReorgVarianceAdjuster(23873.0),
                new SegmentTieredReorgVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.96, ["south"] = 1.33 }),
            },
            new List<string>(),
            4.2,
            90781.5);
    }

    public double Evaluate(TieredReorgVariance item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredReorgVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
