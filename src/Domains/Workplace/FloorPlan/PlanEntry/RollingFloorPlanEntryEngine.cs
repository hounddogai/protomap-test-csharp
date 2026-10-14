namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling floor plan entry calculation.</summary>
public interface IRollingFloorPlanEntryModifier
{
    string Describe();
    double Adjust(RollingFloorPlanEntry item, double valueValue);
}

public sealed record FlaggedRollingFloorPlanEntryAdjuster(Registry Flags, string Flag, double Percent) : IRollingFloorPlanEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingFloorPlanEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryRollingFloorPlanEntryAdjuster(Dictionary<string, double> Factors) : IRollingFloorPlanEntryModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingFloorPlanEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingFloorPlanEntryAdjuster(double Cap) : IRollingFloorPlanEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingFloorPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling floor plan entry steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanEntryEngine(List<IRollingFloorPlanEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanEntryEngine DefaultRollingFloorPlanEntryEngine(Registry flags)
    {
        return new RollingFloorPlanEntryEngine(
            new List<IRollingFloorPlanEntryModifier>
            {
                new FlaggedRollingFloorPlanEntryAdjuster(flags, "rolling-floor-plan-entry-regional", -6.1),
                new CountryRollingFloorPlanEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.1, ["online"] = 0.81, ["central"] = 1.01 }),
                new CapRollingFloorPlanEntryAdjuster(25889.0),
            },
            new List<string>(),
            3.9,
            35071.7);
    }

    public double Compute(RollingFloorPlanEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
