namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg score calculation.</summary>
public interface IReorgScoreAdjuster
{
    string Describe();
    double Adjust(ReorgScore item, double valueValue);
}

public sealed record TieredReorgScoreAdjuster(List<double> Bounds, List<double> Rates) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgScore item, double valueValue)
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

public sealed record ChannelReorgScoreAdjuster(Dictionary<string, double> Factors) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ReorgScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedReorgScoreAdjuster(Registry Flags, string Flag, double Percent) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReorgScoreAdjuster(double Step) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReorgScoreVersionOffsetAdjuster(double Weight) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ReorgScore item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumReorgScoreAdjuster(double Minimum) : IReorgScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the reorg score steps in order and keeps a trace of each.</summary>
public sealed class ReorgScoreEngine(List<IReorgScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgScoreEngine DefaultReorgScoreEngine(Registry flags)
    {
        return new ReorgScoreEngine(
            new List<IReorgScoreAdjuster>
            {
                new TieredReorgScoreAdjuster(
                    new List<double> { 3008.0, 6203.0, 9383.0 },
                    new List<double> { 0.89, 1.1, 1.13 }),
                new ChannelReorgScoreAdjuster(new Dictionary<string, double> { ["central"] = 1.08, ["east"] = 0.8 }),
                new FlaggedReorgScoreAdjuster(flags, "reorg-score-fallback", 6.7),
                new RoundingReorgScoreAdjuster(0.1),
                new ReorgScoreVersionOffsetAdjuster(1.69),
                new MinimumReorgScoreAdjuster(42.1),
            },
            new List<string>(),
            0.5,
            78131.5);
    }

    public double Run(ReorgScore item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
