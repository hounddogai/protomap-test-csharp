namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan group calculation.</summary>
public interface IActionPlanGroupFactor
{
    string Describe();
    double Adjust(ActionPlanGroup item, double valueValue);
}

public sealed record RoundingActionPlanGroupAdjuster(double Step) : IActionPlanGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumActionPlanGroupAdjuster(double Minimum) : IActionPlanGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ActionPlanGroupFloorValueOffsetAdjuster(double Weight) : IActionPlanGroupFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ActionPlanGroup item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedActionPlanGroupAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanGroupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan group steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanGroupEngine(List<IActionPlanGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanGroupEngine DefaultActionPlanGroupEngine(Registry flags)
    {
        return new ActionPlanGroupEngine(
            new List<IActionPlanGroupFactor>
            {
                new RoundingActionPlanGroupAdjuster(1.0),
                new MinimumActionPlanGroupAdjuster(36.6),
                new ActionPlanGroupFloorValueOffsetAdjuster(1.36),
                new FlaggedActionPlanGroupAdjuster(flags, "action-plan-group-tiered", 3.8),
            },
            new List<string>(),
            3.4,
            51712.7);
    }

    public double Run(ActionPlanGroup item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
