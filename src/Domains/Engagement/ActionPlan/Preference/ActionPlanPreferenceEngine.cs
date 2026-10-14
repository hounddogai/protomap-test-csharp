namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan preference calculation.</summary>
public interface IActionPlanPreferenceAdjuster
{
    string Describe();
    double Adjust(ActionPlanPreference item, double valueValue);
}

public sealed record CapActionPlanPreferenceAdjuster(double Cap) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedActionPlanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ActionPlanPreferenceFloorValueOffsetAdjuster(double Weight) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record NameActionPlanPreferenceAdjuster(Dictionary<string, double> Factors) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredActionPlanPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
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

public sealed record PercentActionPlanPreferenceAdjuster(double Percent) : IActionPlanPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan preference steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanPreferenceEngine(List<IActionPlanPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanPreferenceEngine DefaultActionPlanPreferenceEngine(Registry flags)
    {
        return new ActionPlanPreferenceEngine(
            new List<IActionPlanPreferenceAdjuster>
            {
                new CapActionPlanPreferenceAdjuster(48706.0),
                new FlaggedActionPlanPreferenceAdjuster(flags, "action-plan-preference-lenient", -9.5),
                new ActionPlanPreferenceFloorValueOffsetAdjuster(0.29),
                new NameActionPlanPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["north"] = 0.8 }),
                new TieredActionPlanPreferenceAdjuster(
                    new List<double> { 585.0, 7345.0, 8771.0 },
                    new List<double> { 1.19, 0.82, 1.02 }),
                new PercentActionPlanPreferenceAdjuster(-21.2),
            },
            new List<string>(),
            4.3,
            2534.2);
    }

    public double Compute(ActionPlanPreference item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
