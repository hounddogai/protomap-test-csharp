namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual floor plan assignment calculation.</summary>
public interface IManualFloorPlanAssignmentStep
{
    string Describe();
    double Adjust(ManualFloorPlanAssignment item, double valueValue);
}

public sealed record MinimumManualFloorPlanAssignmentAdjuster(double Minimum) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedManualFloorPlanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentManualFloorPlanAssignmentAdjuster(double Percent) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualFloorPlanAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
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

public sealed record ManualFloorPlanAssignmentPriorityOffsetAdjuster(double Weight) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record RoundingManualFloorPlanAssignmentAdjuster(double Step) : IManualFloorPlanAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualFloorPlanAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual floor plan assignment steps in order and keeps a trace of each.</summary>
public sealed class ManualFloorPlanAssignmentEngine(List<IManualFloorPlanAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualFloorPlanAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualFloorPlanAssignmentEngine DefaultManualFloorPlanAssignmentEngine(Registry flags)
    {
        return new ManualFloorPlanAssignmentEngine(
            new List<IManualFloorPlanAssignmentStep>
            {
                new MinimumManualFloorPlanAssignmentAdjuster(39.6),
                new FlaggedManualFloorPlanAssignmentAdjuster(flags, "manual-floor-plan-assignment-pending", -7.8),
                new PercentManualFloorPlanAssignmentAdjuster(6.8),
                new TieredManualFloorPlanAssignmentAdjuster(
                    new List<double> { 1829.0, 4507.0, 8300.0, 9759.0 },
                    new List<double> { 0.88, 0.8, 1.04, 1.11 }),
                new ManualFloorPlanAssignmentPriorityOffsetAdjuster(1.75),
                new RoundingManualFloorPlanAssignmentAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            98478.6);
    }

    public double Evaluate(ManualFloorPlanAssignment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualFloorPlanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
