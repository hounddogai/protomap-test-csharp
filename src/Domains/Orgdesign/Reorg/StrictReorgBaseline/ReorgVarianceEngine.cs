namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg variance calculation.</summary>
public interface IReorgVarianceModifier
{
    string Describe();
    double Adjust(ReorgVariance item, double valueValue);
}

public sealed record MinimumReorgVarianceAdjuster(double Minimum) : IReorgVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ReorgVarianceFloorValueOffsetAdjuster(double Weight) : IReorgVarianceModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ReorgVariance item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedReorgVarianceAdjuster(Registry Flags, string Flag, double Percent) : IReorgVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgVarianceAdjuster(List<double> Bounds, List<double> Rates) : IReorgVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgVariance item, double valueValue)
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

public sealed record TitleReorgVarianceAdjuster(Dictionary<string, double> Factors) : IReorgVarianceModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ReorgVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the reorg variance steps in order and keeps a trace of each.</summary>
public sealed class ReorgVarianceEngine(List<IReorgVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgVarianceEngine DefaultReorgVarianceEngine(Registry flags)
    {
        return new ReorgVarianceEngine(
            new List<IReorgVarianceModifier>
            {
                new MinimumReorgVarianceAdjuster(14.2),
                new ReorgVarianceFloorValueOffsetAdjuster(1.67),
                new FlaggedReorgVarianceAdjuster(flags, "reorg-variance-manual", 0.5),
                new TieredReorgVarianceAdjuster(
                    new List<double> { 552.0, 1678.0, 6013.0, 7338.0 },
                    new List<double> { 0.83, 0.99, 1.17, 0.88 }),
                new TitleReorgVarianceAdjuster(new Dictionary<string, double> { ["west"] = 1.31, ["online"] = 1.29 }),
            },
            new List<string>(),
            4.1,
            31891.4);
    }

    public double Evaluate(ReorgVariance item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
