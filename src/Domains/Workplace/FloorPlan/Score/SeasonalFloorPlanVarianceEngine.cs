namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal floor plan variance calculation.</summary>
public interface ISeasonalFloorPlanVarianceModifier
{
    string Describe();
    double Adjust(SeasonalFloorPlanVariance item, double valueValue);
}

public sealed record PercentSeasonalFloorPlanVarianceAdjuster(double Percent) : ISeasonalFloorPlanVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalFloorPlanVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalFloorPlanVarianceAdjuster(double Cap) : ISeasonalFloorPlanVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFloorPlanVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalFloorPlanVarianceWeightOffsetAdjuster(double Weight) : ISeasonalFloorPlanVarianceModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SeasonalFloorPlanVariance item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the seasonal floor plan variance steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFloorPlanVarianceEngine(List<ISeasonalFloorPlanVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFloorPlanVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFloorPlanVarianceEngine DefaultSeasonalFloorPlanVarianceEngine()
    {
        return new SeasonalFloorPlanVarianceEngine(
            new List<ISeasonalFloorPlanVarianceModifier>
            {
                new PercentSeasonalFloorPlanVarianceAdjuster(-22.0),
                new CapSeasonalFloorPlanVarianceAdjuster(22859.0),
                new SeasonalFloorPlanVarianceWeightOffsetAdjuster(0.1),
            },
            new List<string>(),
            4.3,
            52599.0);
    }

    public double Run(SeasonalFloorPlanVariance item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFloorPlanVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
