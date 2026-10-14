namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan rollup calculation.</summary>
public interface IActionPlanRollupStep
{
    string Describe();
    double Adjust(ActionPlanRollup item, double valueValue);
}

public sealed record FlaggedActionPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ActionPlanRollupWeightOffsetAdjuster(double Weight) : IActionPlanRollupStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ActionPlanRollup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumActionPlanRollupAdjuster(double Minimum) : IActionPlanRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingActionPlanRollupAdjuster(double Step) : IActionPlanRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan rollup steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanRollupEngine(List<IActionPlanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanRollupEngine DefaultActionPlanRollupEngine(Registry flags)
    {
        return new ActionPlanRollupEngine(
            new List<IActionPlanRollupStep>
            {
                new FlaggedActionPlanRollupAdjuster(flags, "action-plan-rollup-legacy", -6.4),
                new ActionPlanRollupWeightOffsetAdjuster(0.61),
                new MinimumActionPlanRollupAdjuster(39.0),
                new RoundingActionPlanRollupAdjuster(0.25),
            },
            new List<string>(),
            4.1,
            72240.5);
    }

    public double Price(ActionPlanRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
