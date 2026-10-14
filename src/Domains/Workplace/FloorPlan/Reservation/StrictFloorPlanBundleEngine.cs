namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict floor plan bundle calculation.</summary>
public interface IStrictFloorPlanBundleStep
{
    string Describe();
    double Adjust(StrictFloorPlanBundle item, double valueValue);
}

public sealed record FlaggedStrictFloorPlanBundleAdjuster(Registry Flags, string Flag, double Percent) : IStrictFloorPlanBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictFloorPlanBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitStrictFloorPlanBundleAdjuster(Dictionary<string, double> Factors) : IStrictFloorPlanBundleStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictFloorPlanBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStrictFloorPlanBundleAdjuster(List<double> Bounds, List<double> Rates) : IStrictFloorPlanBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictFloorPlanBundle item, double valueValue)
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

public sealed record CapStrictFloorPlanBundleAdjuster(double Cap) : IStrictFloorPlanBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictFloorPlanBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict floor plan bundle steps in order and keeps a trace of each.</summary>
public sealed class StrictFloorPlanBundleEngine(List<IStrictFloorPlanBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictFloorPlanBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictFloorPlanBundleEngine DefaultStrictFloorPlanBundleEngine(Registry flags)
    {
        return new StrictFloorPlanBundleEngine(
            new List<IStrictFloorPlanBundleStep>
            {
                new FlaggedStrictFloorPlanBundleAdjuster(flags, "strict-floor-plan-bundle-scheduled", 2.7),
                new UnitStrictFloorPlanBundleAdjuster(new Dictionary<string, double> { ["south"] = 1.11 }),
                new TieredStrictFloorPlanBundleAdjuster(
                    new List<double> { 2306.0, 8340.0 },
                    new List<double> { 0.91, 0.97 }),
                new CapStrictFloorPlanBundleAdjuster(40902.0),
            },
            new List<string>(),
            4.7,
            73747.7);
    }

    public double Run(StrictFloorPlanBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictFloorPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
