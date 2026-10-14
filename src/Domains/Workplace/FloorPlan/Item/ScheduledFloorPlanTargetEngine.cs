namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled floor plan target calculation.</summary>
public interface IScheduledFloorPlanTargetFactor
{
    string Describe();
    double Adjust(ScheduledFloorPlanTarget item, double valueValue);
}

public sealed record CapScheduledFloorPlanTargetAdjuster(double Cap) : IScheduledFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledFloorPlanTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledFloorPlanTargetAdjuster(List<double> Bounds, List<double> Rates) : IScheduledFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledFloorPlanTarget item, double valueValue)
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

public sealed record MinimumScheduledFloorPlanTargetAdjuster(double Minimum) : IScheduledFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFloorPlanTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryScheduledFloorPlanTargetAdjuster(Dictionary<string, double> Factors) : IScheduledFloorPlanTargetFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ScheduledFloorPlanTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingScheduledFloorPlanTargetAdjuster(double Step) : IScheduledFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledFloorPlanTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled floor plan target steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFloorPlanTargetEngine(List<IScheduledFloorPlanTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFloorPlanTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFloorPlanTargetEngine DefaultScheduledFloorPlanTargetEngine()
    {
        return new ScheduledFloorPlanTargetEngine(
            new List<IScheduledFloorPlanTargetFactor>
            {
                new CapScheduledFloorPlanTargetAdjuster(15907.0),
                new TieredScheduledFloorPlanTargetAdjuster(
                    new List<double> { 5870.0, 6092.0, 8499.0, 9130.0 },
                    new List<double> { 1.08, 0.81, 0.83, 1.2 }),
                new MinimumScheduledFloorPlanTargetAdjuster(1.0),
                new CategoryScheduledFloorPlanTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.04, ["retail"] = 1.34 }),
                new RoundingScheduledFloorPlanTargetAdjuster(1.0),
            },
            new List<string>(),
            2.4,
            15841.1);
    }

    public double Evaluate(ScheduledFloorPlanTarget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFloorPlanTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
