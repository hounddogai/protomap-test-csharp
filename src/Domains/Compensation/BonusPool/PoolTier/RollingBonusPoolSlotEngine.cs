namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling bonus pool slot calculation.</summary>
public interface IRollingBonusPoolSlotAdjuster
{
    string Describe();
    double Adjust(RollingBonusPoolSlot item, double valueValue);
}

public sealed record SourceRollingBonusPoolSlotAdjuster(Dictionary<string, double> Factors) : IRollingBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingBonusPoolSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingBonusPoolSlotAdjuster(double Percent) : IRollingBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingBonusPoolSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingBonusPoolSlotRateOffsetAdjuster(double Weight) : IRollingBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RollingBonusPoolSlot item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedRollingBonusPoolSlotAdjuster(Registry Flags, string Flag, double Percent) : IRollingBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBonusPoolSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling bonus pool slot steps in order and keeps a trace of each.</summary>
public sealed class RollingBonusPoolSlotEngine(List<IRollingBonusPoolSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBonusPoolSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBonusPoolSlotEngine DefaultRollingBonusPoolSlotEngine(Registry flags)
    {
        return new RollingBonusPoolSlotEngine(
            new List<IRollingBonusPoolSlotAdjuster>
            {
                new SourceRollingBonusPoolSlotAdjuster(new Dictionary<string, double> { ["central"] = 1.2 }),
                new PercentRollingBonusPoolSlotAdjuster(-11.8),
                new RollingBonusPoolSlotRateOffsetAdjuster(0.66),
                new FlaggedRollingBonusPoolSlotAdjuster(flags, "rolling-bonus-pool-slot-adaptive", 6.0),
            },
            new List<string>(),
            3.4,
            59217.6);
    }

    public double Evaluate(RollingBonusPoolSlot item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBonusPoolSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
