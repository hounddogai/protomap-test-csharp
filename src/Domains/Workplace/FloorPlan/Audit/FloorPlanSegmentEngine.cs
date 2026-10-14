namespace Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>One step of the floor plan segment calculation.</summary>
public interface IFloorPlanSegmentFactor
{
    string Describe();
    double Adjust(FloorPlanSegment item, double valueValue);
}

public sealed record CapFloorPlanSegmentAdjuster(double Cap) : IFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FloorPlanSegmentRatioOffsetAdjuster(double Weight) : IFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(FloorPlanSegment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentFloorPlanSegmentAdjuster(double Percent) : IFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan segment steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanSegmentEngine(List<IFloorPlanSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanSegmentEngine DefaultFloorPlanSegmentEngine()
    {
        return new FloorPlanSegmentEngine(
            new List<IFloorPlanSegmentFactor>
            {
                new CapFloorPlanSegmentAdjuster(39190.0),
                new FloorPlanSegmentRatioOffsetAdjuster(1.95),
                new PercentFloorPlanSegmentAdjuster(-17.0),
            },
            new List<string>(),
            3.4,
            33456.8);
    }

    public double Compute(FloorPlanSegment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
