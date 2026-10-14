namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy floor plan rule calculation.</summary>
public interface ILegacyFloorPlanRuleAdjuster
{
    string Describe();
    double Adjust(LegacyFloorPlanRule item, double valueValue);
}

public sealed record LegacyFloorPlanRuleWeightOffsetAdjuster(double Weight) : ILegacyFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(LegacyFloorPlanRule item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumLegacyFloorPlanRuleAdjuster(double Minimum) : ILegacyFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyFloorPlanRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelLegacyFloorPlanRuleAdjuster(Dictionary<string, double> Factors) : ILegacyFloorPlanRuleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyFloorPlanRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy floor plan rule steps in order and keeps a trace of each.</summary>
public sealed class LegacyFloorPlanRuleEngine(List<ILegacyFloorPlanRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyFloorPlanRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyFloorPlanRuleEngine DefaultLegacyFloorPlanRuleEngine()
    {
        return new LegacyFloorPlanRuleEngine(
            new List<ILegacyFloorPlanRuleAdjuster>
            {
                new LegacyFloorPlanRuleWeightOffsetAdjuster(1.89),
                new MinimumLegacyFloorPlanRuleAdjuster(26.3),
                new LabelLegacyFloorPlanRuleAdjuster(new Dictionary<string, double> { ["central"] = 0.91 }),
            },
            new List<string>(),
            2.9,
            59075.7);
    }

    public double Evaluate(LegacyFloorPlanRule item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyFloorPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
