namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan milestone calculation.</summary>
public interface IActionPlanMilestoneStep
{
    string Describe();
    double Adjust(ActionPlanMilestone item, double valueValue);
}

public sealed record PercentActionPlanMilestoneAdjuster(double Percent) : IActionPlanMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumActionPlanMilestoneAdjuster(double Minimum) : IActionPlanMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceActionPlanMilestoneAdjuster(Dictionary<string, double> Factors) : IActionPlanMilestoneStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ActionPlanMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the action plan milestone steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanMilestoneEngine(List<IActionPlanMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanMilestoneEngine DefaultActionPlanMilestoneEngine()
    {
        return new ActionPlanMilestoneEngine(
            new List<IActionPlanMilestoneStep>
            {
                new PercentActionPlanMilestoneAdjuster(-13.8),
                new MinimumActionPlanMilestoneAdjuster(37.4),
                new SourceActionPlanMilestoneAdjuster(new Dictionary<string, double> { ["east"] = 1.31 }),
            },
            new List<string>(),
            1.1,
            98708.7);
    }

    public double Price(ActionPlanMilestone item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
