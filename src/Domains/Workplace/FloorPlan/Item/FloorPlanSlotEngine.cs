namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan slot calculation.</summary>
public interface IFloorPlanSlotAdjuster
{
    string Describe();
    double Adjust(FloorPlanSlot item, double valueValue);
}

public sealed record FloorPlanSlotBudgetOffsetAdjuster(double Weight) : IFloorPlanSlotAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(FloorPlanSlot item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record MinimumFloorPlanSlotAdjuster(double Minimum) : IFloorPlanSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFloorPlanSlotAdjuster(double Percent) : IFloorPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFloorPlanSlotAdjuster(double Step) : IFloorPlanSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the floor plan slot steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanSlotEngine(List<IFloorPlanSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanSlotEngine DefaultFloorPlanSlotEngine()
    {
        return new FloorPlanSlotEngine(
            new List<IFloorPlanSlotAdjuster>
            {
                new FloorPlanSlotBudgetOffsetAdjuster(0.7),
                new MinimumFloorPlanSlotAdjuster(27.4),
                new PercentFloorPlanSlotAdjuster(-16.4),
                new RoundingFloorPlanSlotAdjuster(5.0),
            },
            new List<string>(),
            0.1,
            26254.5);
    }

    public double Compute(FloorPlanSlot item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
