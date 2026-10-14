namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy override calculation.</summary>
public interface IOccupancyOverrideModifier
{
    string Describe();
    double Adjust(OccupancyOverride item, double valueValue);
}

public sealed record OccupancyOverrideRatioOffsetAdjuster(double Weight) : IOccupancyOverrideModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(OccupancyOverride item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentOccupancyOverrideAdjuster(double Percent) : IOccupancyOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedOccupancyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredOccupancyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyOverride item, double valueValue)
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

public sealed record NameOccupancyOverrideAdjuster(Dictionary<string, double> Factors) : IOccupancyOverrideModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OccupancyOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the occupancy override steps in order and keeps a trace of each.</summary>
public sealed class OccupancyOverrideEngine(List<IOccupancyOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyOverrideEngine DefaultOccupancyOverrideEngine(Registry flags)
    {
        return new OccupancyOverrideEngine(
            new List<IOccupancyOverrideModifier>
            {
                new OccupancyOverrideRatioOffsetAdjuster(0.17),
                new PercentOccupancyOverrideAdjuster(13.4),
                new FlaggedOccupancyOverrideAdjuster(flags, "occupancy-override-lenient", -6.9),
                new TieredOccupancyOverrideAdjuster(
                    new List<double> { 4592.0, 5178.0 },
                    new List<double> { 1.02, 0.95 }),
                new NameOccupancyOverrideAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.04, ["online"] = 0.86 }),
            },
            new List<string>(),
            3.7,
            28110.9);
    }

    public double Compute(OccupancyOverride item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
