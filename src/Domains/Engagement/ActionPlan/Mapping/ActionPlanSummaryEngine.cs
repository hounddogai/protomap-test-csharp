namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan summary calculation.</summary>
public interface IActionPlanSummaryStep
{
    string Describe();
    double Adjust(ActionPlanSummary item, double valueValue);
}

public sealed record NameActionPlanSummaryAdjuster(Dictionary<string, double> Factors) : IActionPlanSummaryStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ActionPlanSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ActionPlanSummarySequenceOffsetAdjuster(double Weight) : IActionPlanSummaryStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(ActionPlanSummary item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingActionPlanSummaryAdjuster(double Step) : IActionPlanSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan summary steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanSummaryEngine(List<IActionPlanSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanSummaryEngine DefaultActionPlanSummaryEngine()
    {
        return new ActionPlanSummaryEngine(
            new List<IActionPlanSummaryStep>
            {
                new NameActionPlanSummaryAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.94, ["north"] = 1.29 }),
                new ActionPlanSummarySequenceOffsetAdjuster(0.95),
                new RoundingActionPlanSummaryAdjuster(5.0),
            },
            new List<string>(),
            2.9,
            57477.8);
    }

    public double Evaluate(ActionPlanSummary item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
