namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan request calculation.</summary>
public interface IActionPlanRequestModifier
{
    string Describe();
    double Adjust(ActionPlanRequest item, double valueValue);
}

public sealed record DepartmentActionPlanRequestAdjuster(Dictionary<string, double> Factors) : IActionPlanRequestModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ActionPlanRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ActionPlanRequestStartsAtOffsetAdjuster(double Weight) : IActionPlanRequestModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(ActionPlanRequest item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record PercentActionPlanRequestAdjuster(double Percent) : IActionPlanRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredActionPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanRequest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the action plan request steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanRequestEngine(List<IActionPlanRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanRequestEngine DefaultActionPlanRequestEngine()
    {
        return new ActionPlanRequestEngine(
            new List<IActionPlanRequestModifier>
            {
                new DepartmentActionPlanRequestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.78, ["central"] = 0.92 }),
                new ActionPlanRequestStartsAtOffsetAdjuster(1.37),
                new PercentActionPlanRequestAdjuster(13.1),
                new TieredActionPlanRequestAdjuster(
                    new List<double> { 1749.0, 3847.0, 4545.0 },
                    new List<double> { 0.98, 0.8, 1.19 }),
            },
            new List<string>(),
            4.1,
            81969.3);
    }

    public double Evaluate(ActionPlanRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
