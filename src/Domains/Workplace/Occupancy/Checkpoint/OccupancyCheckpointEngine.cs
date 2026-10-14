namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy checkpoint calculation.</summary>
public interface IOccupancyCheckpointStep
{
    string Describe();
    double Adjust(OccupancyCheckpoint item, double valueValue);
}

public sealed record RoundingOccupancyCheckpointAdjuster(double Step) : IOccupancyCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record OccupancyCheckpointFloorValueOffsetAdjuster(double Weight) : IOccupancyCheckpointStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(OccupancyCheckpoint item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumOccupancyCheckpointAdjuster(double Minimum) : IOccupancyCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the occupancy checkpoint steps in order and keeps a trace of each.</summary>
public sealed class OccupancyCheckpointEngine(List<IOccupancyCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyCheckpointEngine DefaultOccupancyCheckpointEngine()
    {
        return new OccupancyCheckpointEngine(
            new List<IOccupancyCheckpointStep>
            {
                new RoundingOccupancyCheckpointAdjuster(5.0),
                new OccupancyCheckpointFloorValueOffsetAdjuster(1.91),
                new MinimumOccupancyCheckpointAdjuster(10.6),
            },
            new List<string>(),
            3.1,
            60884.1);
    }

    public double Evaluate(OccupancyCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
