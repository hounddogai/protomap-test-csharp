namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy tier calculation.</summary>
public interface IOccupancyTierStep
{
    string Describe();
    double Adjust(OccupancyTier item, double valueValue);
}

public sealed record CapOccupancyTierAdjuster(double Cap) : IOccupancyTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentOccupancyTierAdjuster(double Percent) : IOccupancyTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOccupancyTierAdjuster(double Minimum) : IOccupancyTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the occupancy tier steps in order and keeps a trace of each.</summary>
public sealed class OccupancyTierEngine(List<IOccupancyTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyTierEngine DefaultOccupancyTierEngine()
    {
        return new OccupancyTierEngine(
            new List<IOccupancyTierStep>
            {
                new CapOccupancyTierAdjuster(785.0),
                new PercentOccupancyTierAdjuster(6.0),
                new MinimumOccupancyTierAdjuster(49.5),
            },
            new List<string>(),
            0.0,
            99372.3);
    }

    public double Run(OccupancyTier item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
