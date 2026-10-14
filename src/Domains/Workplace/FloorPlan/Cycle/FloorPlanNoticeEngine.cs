namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan notice calculation.</summary>
public interface IFloorPlanNoticeAdjuster
{
    string Describe();
    double Adjust(FloorPlanNotice item, double valueValue);
}

public sealed record PercentFloorPlanNoticeAdjuster(double Percent) : IFloorPlanNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFloorPlanNoticeAdjuster(double Step) : IFloorPlanNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFloorPlanNoticeAdjuster(double Minimum) : IFloorPlanNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the floor plan notice steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanNoticeEngine(List<IFloorPlanNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanNoticeEngine DefaultFloorPlanNoticeEngine()
    {
        return new FloorPlanNoticeEngine(
            new List<IFloorPlanNoticeAdjuster>
            {
                new PercentFloorPlanNoticeAdjuster(16.3),
                new RoundingFloorPlanNoticeAdjuster(0.05),
                new MinimumFloorPlanNoticeAdjuster(28.4),
            },
            new List<string>(),
            0.8,
            7285.2);
    }

    public double Run(FloorPlanNotice item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
