namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan item calculation.</summary>
public interface IActionPlanItemStep
{
    string Describe();
    double Adjust(ActionPlanItem item, double valueValue);
}

public sealed record FlaggedActionPlanItemAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanItemStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingActionPlanItemAdjuster(double Step) : IActionPlanItemStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleActionPlanItemAdjuster(Dictionary<string, double> Factors) : IActionPlanItemStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumActionPlanItemAdjuster(double Minimum) : IActionPlanItemStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapActionPlanItemAdjuster(double Cap) : IActionPlanItemStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ActionPlanItemAmountOffsetAdjuster(double Weight) : IActionPlanItemStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ActionPlanItem item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the action plan item steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanItemEngine(List<IActionPlanItemStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanItemStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanItemEngine DefaultActionPlanItemEngine(Registry flags)
    {
        return new ActionPlanItemEngine(
            new List<IActionPlanItemStep>
            {
                new FlaggedActionPlanItemAdjuster(flags, "action-plan-item-scheduled", 8.0),
                new RoundingActionPlanItemAdjuster(0.25),
                new LocaleActionPlanItemAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.87, ["retail"] = 1.14, ["online"] = 0.76 }),
                new MinimumActionPlanItemAdjuster(2.0),
                new CapActionPlanItemAdjuster(13794.0),
                new ActionPlanItemAmountOffsetAdjuster(0.78),
            },
            new List<string>(),
            2.0,
            69489.6);
    }

    public double Run(ActionPlanItem item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
