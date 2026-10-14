namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan rule calculation.</summary>
public interface IFloorPlanRuleAdjuster
{
    string Describe();
    double Adjust(FloorPlanRule item, double valueValue);
}

public sealed record FloorPlanRuleDiscountOffsetAdjuster(double Weight) : IFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(FloorPlanRule item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RegionFloorPlanRuleAdjuster(Dictionary<string, double> Factors) : IFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FloorPlanRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFloorPlanRuleAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFloorPlanRuleAdjuster(double Minimum) : IFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the floor plan rule steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanRuleEngine(List<IFloorPlanRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanRuleEngine DefaultFloorPlanRuleEngine(Registry flags)
    {
        return new FloorPlanRuleEngine(
            new List<IFloorPlanRuleAdjuster>
            {
                new FloorPlanRuleDiscountOffsetAdjuster(0.17),
                new RegionFloorPlanRuleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.1, ["north"] = 0.94, ["online"] = 1.24 }),
                new FlaggedFloorPlanRuleAdjuster(flags, "floor-plan-rule-scheduled", -3.0),
                new MinimumFloorPlanRuleAdjuster(1.3),
            },
            new List<string>(),
            3.0,
            75450.1);
    }

    public double Evaluate(FloorPlanRule item)
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
