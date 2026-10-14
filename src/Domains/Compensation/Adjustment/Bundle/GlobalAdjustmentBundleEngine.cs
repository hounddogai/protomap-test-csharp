namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global adjustment bundle calculation.</summary>
public interface IGlobalAdjustmentBundleStep
{
    string Describe();
    double Adjust(GlobalAdjustmentBundle item, double valueValue);
}

public sealed record TieredGlobalAdjustmentBundleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAdjustmentBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAdjustmentBundle item, double valueValue)
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

public sealed record RoundingGlobalAdjustmentBundleAdjuster(double Step) : IGlobalAdjustmentBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAdjustmentBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalAdjustmentBundleAdjuster(double Percent) : IGlobalAdjustmentBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAdjustmentBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGlobalAdjustmentBundleAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAdjustmentBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAdjustmentBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global adjustment bundle steps in order and keeps a trace of each.</summary>
public sealed class GlobalAdjustmentBundleEngine(List<IGlobalAdjustmentBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAdjustmentBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAdjustmentBundleEngine DefaultGlobalAdjustmentBundleEngine(Registry flags)
    {
        return new GlobalAdjustmentBundleEngine(
            new List<IGlobalAdjustmentBundleStep>
            {
                new TieredGlobalAdjustmentBundleAdjuster(
                    new List<double> { 5535.0, 6089.0 },
                    new List<double> { 1.11, 1.09 }),
                new RoundingGlobalAdjustmentBundleAdjuster(0.5),
                new PercentGlobalAdjustmentBundleAdjuster(23.6),
                new FlaggedGlobalAdjustmentBundleAdjuster(flags, "global-adjustment-bundle-lenient", 9.5),
            },
            new List<string>(),
            0.1,
            40421.6);
    }

    public double Evaluate(GlobalAdjustmentBundle item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAdjustmentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
