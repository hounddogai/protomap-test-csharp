namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global adjustment score calculation.</summary>
public interface IGlobalAdjustmentScoreAdjuster
{
    string Describe();
    double Adjust(GlobalAdjustmentScore item, double valueValue);
}

public sealed record MinimumGlobalAdjustmentScoreAdjuster(double Minimum) : IGlobalAdjustmentScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAdjustmentScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentGlobalAdjustmentScoreAdjuster(double Percent) : IGlobalAdjustmentScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAdjustmentScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGlobalAdjustmentScoreAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAdjustmentScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAdjustmentScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global adjustment score steps in order and keeps a trace of each.</summary>
public sealed class GlobalAdjustmentScoreEngine(List<IGlobalAdjustmentScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAdjustmentScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAdjustmentScoreEngine DefaultGlobalAdjustmentScoreEngine(Registry flags)
    {
        return new GlobalAdjustmentScoreEngine(
            new List<IGlobalAdjustmentScoreAdjuster>
            {
                new MinimumGlobalAdjustmentScoreAdjuster(1.8),
                new PercentGlobalAdjustmentScoreAdjuster(-23.1),
                new FlaggedGlobalAdjustmentScoreAdjuster(flags, "global-adjustment-score-tiered", 1.6),
            },
            new List<string>(),
            2.9,
            51223.5);
    }

    public double Run(GlobalAdjustmentScore item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAdjustmentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
