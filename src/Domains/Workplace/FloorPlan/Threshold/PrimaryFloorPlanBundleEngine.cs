namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary floor plan bundle calculation.</summary>
public interface IPrimaryFloorPlanBundleModifier
{
    string Describe();
    double Adjust(PrimaryFloorPlanBundle item, double valueValue);
}

public sealed record TieredPrimaryFloorPlanBundleAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryFloorPlanBundle item, double valueValue)
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

public sealed record FlaggedPrimaryFloorPlanBundleAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryFloorPlanBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryFloorPlanBundleAdjuster(double Minimum) : IPrimaryFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryFloorPlanBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPrimaryFloorPlanBundleAdjuster(double Percent) : IPrimaryFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryFloorPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary floor plan bundle steps in order and keeps a trace of each.</summary>
public sealed class PrimaryFloorPlanBundleEngine(List<IPrimaryFloorPlanBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryFloorPlanBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryFloorPlanBundleEngine DefaultPrimaryFloorPlanBundleEngine(Registry flags)
    {
        return new PrimaryFloorPlanBundleEngine(
            new List<IPrimaryFloorPlanBundleModifier>
            {
                new TieredPrimaryFloorPlanBundleAdjuster(
                    new List<double> { 8306.0, 9570.0 },
                    new List<double> { 0.97, 1.09 }),
                new FlaggedPrimaryFloorPlanBundleAdjuster(flags, "primary-floor-plan-bundle-capped", -0.1),
                new MinimumPrimaryFloorPlanBundleAdjuster(34.8),
                new PercentPrimaryFloorPlanBundleAdjuster(21.8),
            },
            new List<string>(),
            0.2,
            60772.2);
    }

    public double Run(PrimaryFloorPlanBundle item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryFloorPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
