namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the action plan bundle calculation.</summary>
public interface IActionPlanBundleStep
{
    string Describe();
    double Adjust(ActionPlanBundle item, double valueValue);
}

public sealed record FlaggedActionPlanBundleAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentActionPlanBundleAdjuster(double Percent) : IActionPlanBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumActionPlanBundleAdjuster(double Minimum) : IActionPlanBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the action plan bundle steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanBundleEngine(List<IActionPlanBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanBundleEngine DefaultActionPlanBundleEngine(Registry flags)
    {
        return new ActionPlanBundleEngine(
            new List<IActionPlanBundleStep>
            {
                new FlaggedActionPlanBundleAdjuster(flags, "action-plan-bundle-global", 0.9),
                new PercentActionPlanBundleAdjuster(10.6),
                new MinimumActionPlanBundleAdjuster(0.6),
            },
            new List<string>(),
            4.0,
            96123.8);
    }

    public double Evaluate(ActionPlanBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
