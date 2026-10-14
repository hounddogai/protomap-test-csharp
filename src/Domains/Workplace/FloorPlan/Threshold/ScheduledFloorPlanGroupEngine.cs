namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled floor plan group calculation.</summary>
public interface IScheduledFloorPlanGroupStep
{
    string Describe();
    double Adjust(ScheduledFloorPlanGroup item, double valueValue);
}

public sealed record PercentScheduledFloorPlanGroupAdjuster(double Percent) : IScheduledFloorPlanGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledFloorPlanGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScheduledFloorPlanGroupAdjuster(Registry Flags, string Flag, double Percent) : IScheduledFloorPlanGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledFloorPlanGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledFloorPlanGroupAdjuster(double Minimum) : IScheduledFloorPlanGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledFloorPlanGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledFloorPlanGroupAdjuster(List<double> Bounds, List<double> Rates) : IScheduledFloorPlanGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledFloorPlanGroup item, double valueValue)
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

public sealed record RoundingScheduledFloorPlanGroupAdjuster(double Step) : IScheduledFloorPlanGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledFloorPlanGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled floor plan group steps in order and keeps a trace of each.</summary>
public sealed class ScheduledFloorPlanGroupEngine(List<IScheduledFloorPlanGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledFloorPlanGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledFloorPlanGroupEngine DefaultScheduledFloorPlanGroupEngine(Registry flags)
    {
        return new ScheduledFloorPlanGroupEngine(
            new List<IScheduledFloorPlanGroupStep>
            {
                new PercentScheduledFloorPlanGroupAdjuster(11.9),
                new FlaggedScheduledFloorPlanGroupAdjuster(flags, "scheduled-floor-plan-group-weighted", 4.6),
                new MinimumScheduledFloorPlanGroupAdjuster(36.1),
                new TieredScheduledFloorPlanGroupAdjuster(
                    new List<double> { 2168.0, 3168.0, 3299.0, 5717.0 },
                    new List<double> { 1.0, 0.95, 1.11, 1.02 }),
                new RoundingScheduledFloorPlanGroupAdjuster(0.1),
            },
            new List<string>(),
            3.5,
            42056.7);
    }

    public double Compute(ScheduledFloorPlanGroup item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledFloorPlanGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
