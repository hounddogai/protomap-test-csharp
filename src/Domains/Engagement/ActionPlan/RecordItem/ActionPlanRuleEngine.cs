namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan rule calculation.</summary>
public interface IActionPlanRuleFactor
{
    string Describe();
    double Adjust(ActionPlanRule item, double valueValue);
}

public sealed record MinimumActionPlanRuleAdjuster(double Minimum) : IActionPlanRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ActionPlanRulePriorityOffsetAdjuster(double Weight) : IActionPlanRuleFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ActionPlanRule item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TieredActionPlanRuleAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanRule item, double valueValue)
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

public sealed record PercentActionPlanRuleAdjuster(double Percent) : IActionPlanRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan rule steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanRuleEngine(List<IActionPlanRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanRuleEngine DefaultActionPlanRuleEngine()
    {
        return new ActionPlanRuleEngine(
            new List<IActionPlanRuleFactor>
            {
                new MinimumActionPlanRuleAdjuster(37.5),
                new ActionPlanRulePriorityOffsetAdjuster(1.23),
                new TieredActionPlanRuleAdjuster(new List<double> { 876.0, 7718.0 }, new List<double> { 1.15, 0.99 }),
                new PercentActionPlanRuleAdjuster(-5.8),
            },
            new List<string>(),
            3.6,
            89471.7);
    }

    public double Evaluate(ActionPlanRule item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
