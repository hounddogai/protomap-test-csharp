namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan request calculation.</summary>
public interface IFloorPlanRequestStep
{
    string Describe();
    double Adjust(FloorPlanRequest item, double valueValue);
}

public sealed record FloorPlanRequestCeilingOffsetAdjuster(double Weight) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record RoundingFloorPlanRequestAdjuster(double Step) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFloorPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
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

public sealed record FlaggedFloorPlanRequestAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanRequestAdjuster(double Cap) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamFloorPlanRequestAdjuster(Dictionary<string, double> Factors) : IFloorPlanRequestStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(FloorPlanRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the floor plan request steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanRequestEngine(List<IFloorPlanRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanRequestEngine DefaultFloorPlanRequestEngine(Registry flags)
    {
        return new FloorPlanRequestEngine(
            new List<IFloorPlanRequestStep>
            {
                new FloorPlanRequestCeilingOffsetAdjuster(1.54),
                new RoundingFloorPlanRequestAdjuster(0.25),
                new TieredFloorPlanRequestAdjuster(
                    new List<double> { 306.0, 2478.0, 5559.0, 6802.0 },
                    new List<double> { 1.01, 0.99, 1.08, 0.94 }),
                new FlaggedFloorPlanRequestAdjuster(flags, "floor-plan-request-batched", 9.9),
                new CapFloorPlanRequestAdjuster(12048.0),
                new TeamFloorPlanRequestAdjuster(new Dictionary<string, double> { ["north"] = 1.34 }),
            },
            new List<string>(),
            1.0,
            61571.2);
    }

    public double Price(FloorPlanRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
