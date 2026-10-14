namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled equity grant rollup calculation.</summary>
public interface IScheduledEquityGrantRollupAdjuster
{
    string Describe();
    double Adjust(ScheduledEquityGrantRollup item, double valueValue);
}

public sealed record ScheduledEquityGrantRollupQuantityOffsetAdjuster(double Weight) : IScheduledEquityGrantRollupAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ScheduledEquityGrantRollup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumScheduledEquityGrantRollupAdjuster(double Minimum) : IScheduledEquityGrantRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledEquityGrantRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelScheduledEquityGrantRollupAdjuster(Dictionary<string, double> Factors) : IScheduledEquityGrantRollupAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledEquityGrantRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledEquityGrantRollupAdjuster(Registry Flags, string Flag, double Percent) : IScheduledEquityGrantRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledEquityGrantRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled equity grant rollup steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEquityGrantRollupEngine(List<IScheduledEquityGrantRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEquityGrantRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEquityGrantRollupEngine DefaultScheduledEquityGrantRollupEngine(Registry flags)
    {
        return new ScheduledEquityGrantRollupEngine(
            new List<IScheduledEquityGrantRollupAdjuster>
            {
                new ScheduledEquityGrantRollupQuantityOffsetAdjuster(1.24),
                new MinimumScheduledEquityGrantRollupAdjuster(13.5),
                new LabelScheduledEquityGrantRollupAdjuster(new Dictionary<string, double> { ["north"] = 1.39 }),
                new FlaggedScheduledEquityGrantRollupAdjuster(flags, "scheduled-equity-grant-rollup-adaptive", -6.8),
            },
            new List<string>(),
            2.7,
            34330.7);
    }

    public double Run(ScheduledEquityGrantRollup item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEquityGrantRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
