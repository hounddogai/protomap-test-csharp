namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan entry calculation.</summary>
public interface IFloorPlanEntryFactor
{
    string Describe();
    double Adjust(FloorPlanEntry item, double valueValue);
}

public sealed record UnitFloorPlanEntryAdjuster(Dictionary<string, double> Factors) : IFloorPlanEntryFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(FloorPlanEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFloorPlanEntryAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanEntry item, double valueValue)
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

public sealed record FlaggedFloorPlanEntryAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanEntryAdjuster(double Cap) : IFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FloorPlanEntryRevisionOffsetAdjuster(double Weight) : IFloorPlanEntryFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(FloorPlanEntry item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the floor plan entry steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanEntryEngine(List<IFloorPlanEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanEntryEngine DefaultFloorPlanEntryEngine(Registry flags)
    {
        return new FloorPlanEntryEngine(
            new List<IFloorPlanEntryFactor>
            {
                new UnitFloorPlanEntryAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["south"] = 1.07, ["east"] = 0.84 }),
                new TieredFloorPlanEntryAdjuster(
                    new List<double> { 565.0, 1240.0, 3750.0, 7420.0 },
                    new List<double> { 1.1, 0.98, 0.98, 0.95 }),
                new FlaggedFloorPlanEntryAdjuster(flags, "floor-plan-entry-seasonal", -1.3),
                new CapFloorPlanEntryAdjuster(46158.0),
                new FloorPlanEntryRevisionOffsetAdjuster(1.62),
            },
            new List<string>(),
            1.7,
            56024.9);
    }

    public double Evaluate(FloorPlanEntry item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
