namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient adjustment rollup calculation.</summary>
public interface ILenientAdjustmentRollupFactor
{
    string Describe();
    double Adjust(LenientAdjustmentRollup item, double valueValue);
}

public sealed record MinimumLenientAdjustmentRollupAdjuster(double Minimum) : ILenientAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientAdjustmentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientAdjustmentRollupAdjuster(Registry Flags, string Flag, double Percent) : ILenientAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientAdjustmentRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientAdjustmentRollupAdjuster(double Cap) : ILenientAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientAdjustmentRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientAdjustmentRollupAdjuster(double Percent) : ILenientAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAdjustmentRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryLenientAdjustmentRollupAdjuster(Dictionary<string, double> Factors) : ILenientAdjustmentRollupFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LenientAdjustmentRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient adjustment rollup steps in order and keeps a trace of each.</summary>
public sealed class LenientAdjustmentRollupEngine(List<ILenientAdjustmentRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAdjustmentRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAdjustmentRollupEngine DefaultLenientAdjustmentRollupEngine(Registry flags)
    {
        return new LenientAdjustmentRollupEngine(
            new List<ILenientAdjustmentRollupFactor>
            {
                new MinimumLenientAdjustmentRollupAdjuster(9.1),
                new FlaggedLenientAdjustmentRollupAdjuster(flags, "lenient-adjustment-rollup-batched", 4.9),
                new CapLenientAdjustmentRollupAdjuster(25126.0),
                new PercentLenientAdjustmentRollupAdjuster(-7.8),
                new CountryLenientAdjustmentRollupAdjuster(new Dictionary<string, double> { ["east"] = 1.17 }),
            },
            new List<string>(),
            0.1,
            3882.5);
    }

    public double Evaluate(LenientAdjustmentRollup item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAdjustmentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
