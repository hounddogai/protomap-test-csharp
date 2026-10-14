namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan preference calculation.</summary>
public interface ISuccessionPlanPreferenceFactor
{
    string Describe();
    double Adjust(SuccessionPlanPreference item, double valueValue);
}

public sealed record PercentSuccessionPlanPreferenceAdjuster(double Percent) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSuccessionPlanPreferenceAdjuster(double Cap) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSuccessionPlanPreferenceAdjuster(double Minimum) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSuccessionPlanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSuccessionPlanPreferenceAdjuster(double Step) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSuccessionPlanPreferenceAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanPreferenceFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SuccessionPlanPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the succession plan preference steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanPreferenceEngine(List<ISuccessionPlanPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanPreferenceEngine DefaultSuccessionPlanPreferenceEngine(Registry flags)
    {
        return new SuccessionPlanPreferenceEngine(
            new List<ISuccessionPlanPreferenceFactor>
            {
                new PercentSuccessionPlanPreferenceAdjuster(-9.9),
                new CapSuccessionPlanPreferenceAdjuster(21595.0),
                new MinimumSuccessionPlanPreferenceAdjuster(44.8),
                new FlaggedSuccessionPlanPreferenceAdjuster(flags, "succession-plan-preference-regional", -7.4),
                new RoundingSuccessionPlanPreferenceAdjuster(0.25),
                new TeamSuccessionPlanPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.21, ["east"] = 1.12, ["central"] = 0.95 }),
            },
            new List<string>(),
            3.7,
            49800.8);
    }

    public double Price(SuccessionPlanPreference item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
