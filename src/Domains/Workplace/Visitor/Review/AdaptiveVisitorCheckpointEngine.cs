namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive visitor checkpoint calculation.</summary>
public interface IAdaptiveVisitorCheckpointAdjuster
{
    string Describe();
    double Adjust(AdaptiveVisitorCheckpoint item, double valueValue);
}

public sealed record CountryAdaptiveVisitorCheckpointAdjuster(Dictionary<string, double> Factors) : IAdaptiveVisitorCheckpointAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveVisitorCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveVisitorCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveVisitorCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveVisitorCheckpoint item, double valueValue)
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

public sealed record CapAdaptiveVisitorCheckpointAdjuster(double Cap) : IAdaptiveVisitorCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveVisitorCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveVisitorCheckpointAdjuster(double Percent) : IAdaptiveVisitorCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveVisitorCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive visitor checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveVisitorCheckpointEngine(List<IAdaptiveVisitorCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveVisitorCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveVisitorCheckpointEngine DefaultAdaptiveVisitorCheckpointEngine()
    {
        return new AdaptiveVisitorCheckpointEngine(
            new List<IAdaptiveVisitorCheckpointAdjuster>
            {
                new CountryAdaptiveVisitorCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.82, ["north"] = 1.27 }),
                new TieredAdaptiveVisitorCheckpointAdjuster(
                    new List<double> { 750.0, 5518.0 },
                    new List<double> { 0.94, 1.0 }),
                new CapAdaptiveVisitorCheckpointAdjuster(5134.0),
                new PercentAdaptiveVisitorCheckpointAdjuster(17.4),
            },
            new List<string>(),
            3.0,
            16391.2);
    }

    public double Evaluate(AdaptiveVisitorCheckpoint item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveVisitorCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
