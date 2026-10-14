namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual occupancy plan calculation.</summary>
public interface IManualOccupancyPlanStep
{
    string Describe();
    double Adjust(ManualOccupancyPlan item, double valueValue);
}

public sealed record CapManualOccupancyPlanAdjuster(double Cap) : IManualOccupancyPlanStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualOccupancyPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumManualOccupancyPlanAdjuster(double Minimum) : IManualOccupancyPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualOccupancyPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualOccupancyPlanAdjuster(double Step) : IManualOccupancyPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualOccupancyPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredManualOccupancyPlanAdjuster(List<double> Bounds, List<double> Rates) : IManualOccupancyPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualOccupancyPlan item, double valueValue)
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

/// <summary>Runs the manual occupancy plan steps in order and keeps a trace of each.</summary>
public sealed class ManualOccupancyPlanEngine(List<IManualOccupancyPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualOccupancyPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualOccupancyPlanEngine DefaultManualOccupancyPlanEngine()
    {
        return new ManualOccupancyPlanEngine(
            new List<IManualOccupancyPlanStep>
            {
                new CapManualOccupancyPlanAdjuster(26883.0),
                new MinimumManualOccupancyPlanAdjuster(35.9),
                new RoundingManualOccupancyPlanAdjuster(0.5),
                new TieredManualOccupancyPlanAdjuster(
                    new List<double> { 238.0, 2548.0, 7639.0, 8681.0 },
                    new List<double> { 0.98, 0.85, 1.1, 1.16 }),
            },
            new List<string>(),
            2.8,
            16090.8);
    }

    public double Price(ManualOccupancyPlan item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualOccupancyPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
