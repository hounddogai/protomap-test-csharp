namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan metric calculation.</summary>
public interface IFloorPlanMetricAdjuster
{
    string Describe();
    double Adjust(FloorPlanMetric item, double valueValue);
}

public sealed record MinimumFloorPlanMetricAdjuster(double Minimum) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FloorPlanMetricCeilingOffsetAdjuster(double Weight) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record RoundingFloorPlanMetricAdjuster(double Step) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFloorPlanMetricAdjuster(double Cap) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameFloorPlanMetricAdjuster(Dictionary<string, double> Factors) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFloorPlanMetricAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan metric steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanMetricEngine(List<IFloorPlanMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanMetricEngine DefaultFloorPlanMetricEngine(Registry flags)
    {
        return new FloorPlanMetricEngine(
            new List<IFloorPlanMetricAdjuster>
            {
                new MinimumFloorPlanMetricAdjuster(1.5),
                new FloorPlanMetricCeilingOffsetAdjuster(1.81),
                new RoundingFloorPlanMetricAdjuster(0.5),
                new CapFloorPlanMetricAdjuster(36429.0),
                new NameFloorPlanMetricAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.72, ["west"] = 0.94 }),
                new FlaggedFloorPlanMetricAdjuster(flags, "floor-plan-metric-global", -9.9),
            },
            new List<string>(),
            3.9,
            19137.9);
    }

    public double Price(FloorPlanMetric item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
