namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual floor plan item calculation.</summary>
public interface IManualFloorPlanItemStep
{
    string Describe();
    double Adjust(ManualFloorPlanItem item, double valueValue);
}

public sealed record FlaggedManualFloorPlanItemAdjuster(Registry Flags, string Flag, double Percent) : IManualFloorPlanItemStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualFloorPlanItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualFloorPlanItemAdjuster(List<double> Bounds, List<double> Rates) : IManualFloorPlanItemStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualFloorPlanItem item, double valueValue)
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

public sealed record MinimumManualFloorPlanItemAdjuster(double Minimum) : IManualFloorPlanItemStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualFloorPlanItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualFloorPlanItemAdjuster(double Cap) : IManualFloorPlanItemStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualFloorPlanItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual floor plan item steps in order and keeps a trace of each.</summary>
public sealed class ManualFloorPlanItemEngine(List<IManualFloorPlanItemStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualFloorPlanItemStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualFloorPlanItemEngine DefaultManualFloorPlanItemEngine(Registry flags)
    {
        return new ManualFloorPlanItemEngine(
            new List<IManualFloorPlanItemStep>
            {
                new FlaggedManualFloorPlanItemAdjuster(flags, "manual-floor-plan-item-global", -0.4),
                new TieredManualFloorPlanItemAdjuster(
                    new List<double> { 30.0, 1382.0 },
                    new List<double> { 1.16, 0.8 }),
                new MinimumManualFloorPlanItemAdjuster(10.9),
                new CapManualFloorPlanItemAdjuster(42134.0),
            },
            new List<string>(),
            2.2,
            34394.4);
    }

    public double Compute(ManualFloorPlanItem item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualFloorPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
