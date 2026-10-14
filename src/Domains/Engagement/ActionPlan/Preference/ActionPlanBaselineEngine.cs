namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan baseline calculation.</summary>
public interface IActionPlanBaselineAdjuster
{
    string Describe();
    double Adjust(ActionPlanBaseline item, double valueValue);
}

public sealed record CategoryActionPlanBaselineAdjuster(Dictionary<string, double> Factors) : IActionPlanBaselineAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ActionPlanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingActionPlanBaselineAdjuster(double Step) : IActionPlanBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentActionPlanBaselineAdjuster(double Percent) : IActionPlanBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredActionPlanBaselineAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanBaseline item, double valueValue)
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

public sealed record FlaggedActionPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan baseline steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanBaselineEngine(List<IActionPlanBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanBaselineEngine DefaultActionPlanBaselineEngine(Registry flags)
    {
        return new ActionPlanBaselineEngine(
            new List<IActionPlanBaselineAdjuster>
            {
                new CategoryActionPlanBaselineAdjuster(new Dictionary<string, double> { ["south"] = 1.11 }),
                new RoundingActionPlanBaselineAdjuster(5.0),
                new PercentActionPlanBaselineAdjuster(-3.4),
                new TieredActionPlanBaselineAdjuster(
                    new List<double> { 927.0, 2527.0, 4345.0, 9649.0 },
                    new List<double> { 1.19, 0.93, 0.81, 1.08 }),
                new FlaggedActionPlanBaselineAdjuster(flags, "action-plan-baseline-default", -6.7),
            },
            new List<string>(),
            4.3,
            28953.5);
    }

    public double Compute(ActionPlanBaseline item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
