namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the bonus pool checkpoint calculation.</summary>
public interface IBonusPoolCheckpointAdjuster
{
    string Describe();
    double Adjust(BonusPoolCheckpoint item, double valueValue);
}

public sealed record PercentBonusPoolCheckpointAdjuster(double Percent) : IBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBonusPoolCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBonusPoolCheckpointAdjuster(Dictionary<string, double> Factors) : IBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BonusPoolCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBonusPoolCheckpointAdjuster(double Minimum) : IBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BonusPoolCheckpointAmountOffsetAdjuster(double Weight) : IBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BonusPoolCheckpoint item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the bonus pool checkpoint steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolCheckpointEngine(List<IBonusPoolCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolCheckpointEngine DefaultBonusPoolCheckpointEngine(Registry flags)
    {
        return new BonusPoolCheckpointEngine(
            new List<IBonusPoolCheckpointAdjuster>
            {
                new PercentBonusPoolCheckpointAdjuster(-13.4),
                new FlaggedBonusPoolCheckpointAdjuster(flags, "bonus-pool-checkpoint-default", -6.5),
                new SegmentBonusPoolCheckpointAdjuster(new Dictionary<string, double> { ["east"] = 1.08 }),
                new MinimumBonusPoolCheckpointAdjuster(4.7),
                new BonusPoolCheckpointAmountOffsetAdjuster(1.77),
            },
            new List<string>(),
            4.9,
            74315.5);
    }

    public double Compute(BonusPoolCheckpoint item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
