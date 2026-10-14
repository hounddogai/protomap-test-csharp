namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global adjustment rollup calculation.</summary>
public interface IGlobalAdjustmentRollupFactor
{
    string Describe();
    double Adjust(GlobalAdjustmentRollup item, double valueValue);
}

public sealed record FlaggedGlobalAdjustmentRollupAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalAdjustmentRollupAdjuster(double Minimum) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGlobalAdjustmentRollupAdjuster(double Step) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalAdjustmentRollupAdjuster(double Percent) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalAdjustmentRollupAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
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

public sealed record CapGlobalAdjustmentRollupAdjuster(double Cap) : IGlobalAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalAdjustmentRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global adjustment rollup steps in order and keeps a trace of each.</summary>
public sealed class GlobalAdjustmentRollupEngine(List<IGlobalAdjustmentRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAdjustmentRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAdjustmentRollupEngine DefaultGlobalAdjustmentRollupEngine(Registry flags)
    {
        return new GlobalAdjustmentRollupEngine(
            new List<IGlobalAdjustmentRollupFactor>
            {
                new FlaggedGlobalAdjustmentRollupAdjuster(flags, "global-adjustment-rollup-adaptive", 7.7),
                new MinimumGlobalAdjustmentRollupAdjuster(15.7),
                new RoundingGlobalAdjustmentRollupAdjuster(5.0),
                new PercentGlobalAdjustmentRollupAdjuster(7.9),
                new TieredGlobalAdjustmentRollupAdjuster(
                    new List<double> { 2510.0, 2896.0, 3560.0 },
                    new List<double> { 0.81, 0.95, 1.05 }),
                new CapGlobalAdjustmentRollupAdjuster(39937.0),
            },
            new List<string>(),
            2.5,
            11621.5);
    }

    public double Evaluate(GlobalAdjustmentRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAdjustmentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
