namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the action plan threshold calculation.</summary>
public interface IActionPlanThresholdStep
{
    string Describe();
    double Adjust(ActionPlanThreshold item, double valueValue);
}

public sealed record ChannelActionPlanThresholdAdjuster(Dictionary<string, double> Factors) : IActionPlanThresholdStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ActionPlanThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentActionPlanThresholdAdjuster(double Percent) : IActionPlanThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedActionPlanThresholdAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan threshold steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanThresholdEngine(List<IActionPlanThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanThresholdEngine DefaultActionPlanThresholdEngine(Registry flags)
    {
        return new ActionPlanThresholdEngine(
            new List<IActionPlanThresholdStep>
            {
                new ChannelActionPlanThresholdAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.18, ["west"] = 0.81 }),
                new PercentActionPlanThresholdAdjuster(-13.4),
                new FlaggedActionPlanThresholdAdjuster(flags, "action-plan-threshold-legacy", -2.9),
            },
            new List<string>(),
            4.8,
            6476.9);
    }

    public double Evaluate(ActionPlanThreshold item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
