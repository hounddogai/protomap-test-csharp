namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default floor plan segment calculation.</summary>
public interface IDefaultFloorPlanSegmentFactor
{
    string Describe();
    double Adjust(DefaultFloorPlanSegment item, double valueValue);
}

public sealed record RoundingDefaultFloorPlanSegmentAdjuster(double Step) : IDefaultFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultFloorPlanSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultFloorPlanSegmentAdjuster(double Percent) : IDefaultFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultFloorPlanSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultFloorPlanSegmentLeaveDaysOffsetAdjuster(double Weight) : IDefaultFloorPlanSegmentFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(DefaultFloorPlanSegment item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the default floor plan segment steps in order and keeps a trace of each.</summary>
public sealed class DefaultFloorPlanSegmentEngine(List<IDefaultFloorPlanSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultFloorPlanSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultFloorPlanSegmentEngine DefaultDefaultFloorPlanSegmentEngine()
    {
        return new DefaultFloorPlanSegmentEngine(
            new List<IDefaultFloorPlanSegmentFactor>
            {
                new RoundingDefaultFloorPlanSegmentAdjuster(0.5),
                new PercentDefaultFloorPlanSegmentAdjuster(-6.1),
                new DefaultFloorPlanSegmentLeaveDaysOffsetAdjuster(1.98),
            },
            new List<string>(),
            2.6,
            34600.3);
    }

    public double Evaluate(DefaultFloorPlanSegment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultFloorPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
