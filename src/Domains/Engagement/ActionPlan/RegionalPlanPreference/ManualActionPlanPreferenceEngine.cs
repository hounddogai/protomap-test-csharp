namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual action plan preference calculation.</summary>
public interface IManualActionPlanPreferenceStep
{
    string Describe();
    double Adjust(ManualActionPlanPreference item, double valueValue);
}

public sealed record CapManualActionPlanPreferenceAdjuster(double Cap) : IManualActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualActionPlanPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentManualActionPlanPreferenceAdjuster(double Percent) : IManualActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualActionPlanPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentManualActionPlanPreferenceAdjuster(Dictionary<string, double> Factors) : IManualActionPlanPreferenceStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ManualActionPlanPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualActionPlanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IManualActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualActionPlanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual action plan preference steps in order and keeps a trace of each.</summary>
public sealed class ManualActionPlanPreferenceEngine(List<IManualActionPlanPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualActionPlanPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualActionPlanPreferenceEngine DefaultManualActionPlanPreferenceEngine(Registry flags)
    {
        return new ManualActionPlanPreferenceEngine(
            new List<IManualActionPlanPreferenceStep>
            {
                new CapManualActionPlanPreferenceAdjuster(35642.0),
                new PercentManualActionPlanPreferenceAdjuster(-9.3),
                new DepartmentManualActionPlanPreferenceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.29, ["east"] = 0.78, ["online"] = 1.23 }),
                new FlaggedManualActionPlanPreferenceAdjuster(flags, "manual-action-plan-preference-rolling", -2.5),
            },
            new List<string>(),
            1.2,
            52944.3);
    }

    public double Compute(ManualActionPlanPreference item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualActionPlanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
