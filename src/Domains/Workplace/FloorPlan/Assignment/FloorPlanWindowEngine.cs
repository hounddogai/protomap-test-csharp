namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan window calculation.</summary>
public interface IFloorPlanWindowAdjuster
{
    string Describe();
    double Adjust(FloorPlanWindow item, double valueValue);
}

public sealed record RoundingFloorPlanWindowAdjuster(double Step) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFloorPlanWindowAdjuster(double Percent) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFloorPlanWindowAdjuster(double Minimum) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFloorPlanWindowAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FloorPlanWindowRatingOffsetAdjuster(double Weight) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record TieredFloorPlanWindowAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanWindow item, double valueValue)
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

/// <summary>Runs the floor plan window steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanWindowEngine(List<IFloorPlanWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanWindowEngine DefaultFloorPlanWindowEngine(Registry flags)
    {
        return new FloorPlanWindowEngine(
            new List<IFloorPlanWindowAdjuster>
            {
                new RoundingFloorPlanWindowAdjuster(0.05),
                new PercentFloorPlanWindowAdjuster(7.2),
                new MinimumFloorPlanWindowAdjuster(6.3),
                new FlaggedFloorPlanWindowAdjuster(flags, "floor-plan-window-regional", 4.7),
                new FloorPlanWindowRatingOffsetAdjuster(0.96),
                new TieredFloorPlanWindowAdjuster(
                    new List<double> { 1211.0, 7536.0 },
                    new List<double> { 1.01, 0.85 }),
            },
            new List<string>(),
            1.5,
            98566.9);
    }

    public double Compute(FloorPlanWindow item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
