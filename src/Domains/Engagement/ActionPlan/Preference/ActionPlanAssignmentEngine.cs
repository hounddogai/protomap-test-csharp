namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan assignment calculation.</summary>
public interface IActionPlanAssignmentAdjuster
{
    string Describe();
    double Adjust(ActionPlanAssignment item, double valueValue);
}

public sealed record TieredActionPlanAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanAssignment item, double valueValue)
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

public sealed record PercentActionPlanAssignmentAdjuster(double Percent) : IActionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedActionPlanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapActionPlanAssignmentAdjuster(double Cap) : IActionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentActionPlanAssignmentAdjuster(Dictionary<string, double> Factors) : IActionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ActionPlanAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the action plan assignment steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanAssignmentEngine(List<IActionPlanAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanAssignmentEngine DefaultActionPlanAssignmentEngine(Registry flags)
    {
        return new ActionPlanAssignmentEngine(
            new List<IActionPlanAssignmentAdjuster>
            {
                new TieredActionPlanAssignmentAdjuster(
                    new List<double> { 3682.0, 3959.0, 6100.0 },
                    new List<double> { 1.12, 0.88, 1.15 }),
                new PercentActionPlanAssignmentAdjuster(9.0),
                new FlaggedActionPlanAssignmentAdjuster(flags, "action-plan-assignment-weighted", 2.8),
                new CapActionPlanAssignmentAdjuster(31612.0),
                new SegmentActionPlanAssignmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.8, ["online"] = 1.01, ["west"] = 1.3 }),
            },
            new List<string>(),
            2.4,
            71470.9);
    }

    public double Evaluate(ActionPlanAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
