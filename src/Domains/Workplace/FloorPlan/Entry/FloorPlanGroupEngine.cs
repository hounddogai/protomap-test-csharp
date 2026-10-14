namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan group calculation.</summary>
public interface IFloorPlanGroupStep
{
    string Describe();
    double Adjust(FloorPlanGroup item, double valueValue);
}

public sealed record RoundingFloorPlanGroupAdjuster(double Step) : IFloorPlanGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFloorPlanGroupAdjuster(double Cap) : IFloorPlanGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FloorPlanGroupRateOffsetAdjuster(double Weight) : IFloorPlanGroupStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(FloorPlanGroup item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the floor plan group steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanGroupEngine(List<IFloorPlanGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanGroupEngine DefaultFloorPlanGroupEngine()
    {
        return new FloorPlanGroupEngine(
            new List<IFloorPlanGroupStep>
            {
                new RoundingFloorPlanGroupAdjuster(1.0),
                new CapFloorPlanGroupAdjuster(34966.0),
                new FloorPlanGroupRateOffsetAdjuster(0.67),
            },
            new List<string>(),
            2.1,
            31012.8);
    }

    public double Run(FloorPlanGroup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
