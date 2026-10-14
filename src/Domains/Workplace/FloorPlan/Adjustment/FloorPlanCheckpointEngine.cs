namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan checkpoint calculation.</summary>
public interface IFloorPlanCheckpointModifier
{
    string Describe();
    double Adjust(FloorPlanCheckpoint item, double valueValue);
}

public sealed record TieredFloorPlanCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanCheckpoint item, double valueValue)
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

public sealed record MinimumFloorPlanCheckpointAdjuster(double Minimum) : IFloorPlanCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFloorPlanCheckpointAdjuster(double Percent) : IFloorPlanCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanCheckpointAdjuster(double Cap) : IFloorPlanCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan checkpoint steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanCheckpointEngine(List<IFloorPlanCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanCheckpointEngine DefaultFloorPlanCheckpointEngine()
    {
        return new FloorPlanCheckpointEngine(
            new List<IFloorPlanCheckpointModifier>
            {
                new TieredFloorPlanCheckpointAdjuster(
                    new List<double> { 7436.0, 7650.0 },
                    new List<double> { 1.05, 0.82 }),
                new MinimumFloorPlanCheckpointAdjuster(6.0),
                new PercentFloorPlanCheckpointAdjuster(0.8),
                new CapFloorPlanCheckpointAdjuster(28610.0),
            },
            new List<string>(),
            1.4,
            76825.8);
    }

    public double Evaluate(FloorPlanCheckpoint item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
