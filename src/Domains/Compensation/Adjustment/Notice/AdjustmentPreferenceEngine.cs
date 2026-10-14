namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment preference calculation.</summary>
public interface IAdjustmentPreferenceModifier
{
    string Describe();
    double Adjust(AdjustmentPreference item, double valueValue);
}

public sealed record PercentAdjustmentPreferenceAdjuster(double Percent) : IAdjustmentPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelAdjustmentPreferenceAdjuster(Dictionary<string, double> Factors) : IAdjustmentPreferenceModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdjustmentPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdjustmentPreferenceAdjuster(double Cap) : IAdjustmentPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdjustmentPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment preference steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentPreferenceEngine(List<IAdjustmentPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentPreferenceEngine DefaultAdjustmentPreferenceEngine(Registry flags)
    {
        return new AdjustmentPreferenceEngine(
            new List<IAdjustmentPreferenceModifier>
            {
                new PercentAdjustmentPreferenceAdjuster(6.9),
                new LabelAdjustmentPreferenceAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.21, ["north"] = 1.09 }),
                new CapAdjustmentPreferenceAdjuster(48582.0),
                new FlaggedAdjustmentPreferenceAdjuster(flags, "adjustment-preference-rolling", -5.4),
            },
            new List<string>(),
            2.6,
            28586.3);
    }

    public double Compute(AdjustmentPreference item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
