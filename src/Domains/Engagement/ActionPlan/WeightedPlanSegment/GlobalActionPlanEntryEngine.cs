namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global action plan entry calculation.</summary>
public interface IGlobalActionPlanEntryModifier
{
    string Describe();
    double Adjust(GlobalActionPlanEntry item, double valueValue);
}

public sealed record RoundingGlobalActionPlanEntryAdjuster(double Step) : IGlobalActionPlanEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalActionPlanEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalActionPlanEntryAdjuster(Registry Flags, string Flag, double Percent) : IGlobalActionPlanEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalActionPlanEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleGlobalActionPlanEntryAdjuster(Dictionary<string, double> Factors) : IGlobalActionPlanEntryModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalActionPlanEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalActionPlanEntryAdjuster(double Minimum) : IGlobalActionPlanEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalActionPlanEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global action plan entry steps in order and keeps a trace of each.</summary>
public sealed class GlobalActionPlanEntryEngine(List<IGlobalActionPlanEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalActionPlanEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalActionPlanEntryEngine DefaultGlobalActionPlanEntryEngine(Registry flags)
    {
        return new GlobalActionPlanEntryEngine(
            new List<IGlobalActionPlanEntryModifier>
            {
                new RoundingGlobalActionPlanEntryAdjuster(0.1),
                new FlaggedGlobalActionPlanEntryAdjuster(flags, "global-action-plan-entry-strict", 3.9),
                new TitleGlobalActionPlanEntryAdjuster(new Dictionary<string, double> { ["east"] = 1.3 }),
                new MinimumGlobalActionPlanEntryAdjuster(20.6),
            },
            new List<string>(),
            4.4,
            90453.3);
    }

    public double Run(GlobalActionPlanEntry item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalActionPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
