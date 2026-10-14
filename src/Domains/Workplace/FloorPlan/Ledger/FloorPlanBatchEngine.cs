namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan batch calculation.</summary>
public interface IFloorPlanBatchAdjuster
{
    string Describe();
    double Adjust(FloorPlanBatch item, double valueValue);
}

public sealed record RoundingFloorPlanBatchAdjuster(double Step) : IFloorPlanBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameFloorPlanBatchAdjuster(Dictionary<string, double> Factors) : IFloorPlanBatchAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FloorPlanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFloorPlanBatchAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanBatchAdjuster(double Cap) : IFloorPlanBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan batch steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanBatchEngine(List<IFloorPlanBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanBatchEngine DefaultFloorPlanBatchEngine(Registry flags)
    {
        return new FloorPlanBatchEngine(
            new List<IFloorPlanBatchAdjuster>
            {
                new RoundingFloorPlanBatchAdjuster(0.5),
                new NameFloorPlanBatchAdjuster(new Dictionary<string, double> { ["west"] = 1.32, ["north"] = 0.86 }),
                new FlaggedFloorPlanBatchAdjuster(flags, "floor-plan-batch-strict", 4.4),
                new CapFloorPlanBatchAdjuster(20938.0),
            },
            new List<string>(),
            1.0,
            66827.8);
    }

    public double Evaluate(FloorPlanBatch item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
