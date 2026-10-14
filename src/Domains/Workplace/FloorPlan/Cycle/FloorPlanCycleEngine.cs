namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan cycle calculation.</summary>
public interface IFloorPlanCycleStep
{
    string Describe();
    double Adjust(FloorPlanCycle item, double valueValue);
}

public sealed record CapFloorPlanCycleAdjuster(double Cap) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFloorPlanCycleAdjuster(double Percent) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FloorPlanCycleRevisionOffsetAdjuster(double Weight) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedFloorPlanCycleAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitFloorPlanCycleAdjuster(Dictionary<string, double> Factors) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFloorPlanCycleAdjuster(double Step) : IFloorPlanCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the floor plan cycle steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanCycleEngine(List<IFloorPlanCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanCycleEngine DefaultFloorPlanCycleEngine(Registry flags)
    {
        return new FloorPlanCycleEngine(
            new List<IFloorPlanCycleStep>
            {
                new CapFloorPlanCycleAdjuster(564.0),
                new PercentFloorPlanCycleAdjuster(-8.9),
                new FloorPlanCycleRevisionOffsetAdjuster(0.54),
                new FlaggedFloorPlanCycleAdjuster(flags, "floor-plan-cycle-regional", 8.4),
                new UnitFloorPlanCycleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.13, ["retail"] = 0.88, ["south"] = 1.02 }),
                new RoundingFloorPlanCycleAdjuster(0.25),
            },
            new List<string>(),
            2.4,
            87142.1);
    }

    public double Price(FloorPlanCycle item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
