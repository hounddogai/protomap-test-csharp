namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy rollup calculation.</summary>
public interface IOccupancyRollupModifier
{
    string Describe();
    double Adjust(OccupancyRollup item, double valueValue);
}

public sealed record MinimumOccupancyRollupAdjuster(double Minimum) : IOccupancyRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyRollupAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelOccupancyRollupAdjuster(Dictionary<string, double> Factors) : IOccupancyRollupModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OccupancyRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the occupancy rollup steps in order and keeps a trace of each.</summary>
public sealed class OccupancyRollupEngine(List<IOccupancyRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyRollupEngine DefaultOccupancyRollupEngine(Registry flags)
    {
        return new OccupancyRollupEngine(
            new List<IOccupancyRollupModifier>
            {
                new MinimumOccupancyRollupAdjuster(32.7),
                new FlaggedOccupancyRollupAdjuster(flags, "occupancy-rollup-strict", 2.7),
                new LabelOccupancyRollupAdjuster(new Dictionary<string, double> { ["online"] = 1.2 }),
            },
            new List<string>(),
            0.6,
            58295.2);
    }

    public double Run(OccupancyRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
