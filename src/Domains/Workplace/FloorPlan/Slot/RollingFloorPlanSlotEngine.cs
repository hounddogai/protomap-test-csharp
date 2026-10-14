namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling floor plan slot calculation.</summary>
public interface IRollingFloorPlanSlotModifier
{
    string Describe();
    double Adjust(RollingFloorPlanSlot item, double valueValue);
}

public sealed record MinimumRollingFloorPlanSlotAdjuster(double Minimum) : IRollingFloorPlanSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFloorPlanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingFloorPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IRollingFloorPlanSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingFloorPlanSlot item, double valueValue)
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

public sealed record PercentRollingFloorPlanSlotAdjuster(double Percent) : IRollingFloorPlanSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingFloorPlanSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingFloorPlanSlotAdjuster(Registry Flags, string Flag, double Percent) : IRollingFloorPlanSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingFloorPlanSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling floor plan slot steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanSlotEngine(List<IRollingFloorPlanSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanSlotEngine DefaultRollingFloorPlanSlotEngine(Registry flags)
    {
        return new RollingFloorPlanSlotEngine(
            new List<IRollingFloorPlanSlotModifier>
            {
                new MinimumRollingFloorPlanSlotAdjuster(11.9),
                new TieredRollingFloorPlanSlotAdjuster(
                    new List<double> { 486.0, 9535.0 },
                    new List<double> { 0.98, 1.2 }),
                new PercentRollingFloorPlanSlotAdjuster(6.1),
                new FlaggedRollingFloorPlanSlotAdjuster(flags, "rolling-floor-plan-slot-default", 7.4),
            },
            new List<string>(),
            4.0,
            31016.2);
    }

    public double Price(RollingFloorPlanSlot item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
