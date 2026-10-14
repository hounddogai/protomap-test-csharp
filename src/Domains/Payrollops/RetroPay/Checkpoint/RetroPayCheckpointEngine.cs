namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>One step of the retro pay checkpoint calculation.</summary>
public interface IRetroPayCheckpointAdjuster
{
    string Describe();
    double Adjust(RetroPayCheckpoint item, double valueValue);
}

public sealed record TieredRetroPayCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayCheckpoint item, double valueValue)
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

public sealed record CapRetroPayCheckpointAdjuster(double Cap) : IRetroPayCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRetroPayCheckpointAdjuster(double Percent) : IRetroPayCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the retro pay checkpoint steps in order and keeps a trace of each.</summary>
public sealed class RetroPayCheckpointEngine(List<IRetroPayCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayCheckpointEngine DefaultRetroPayCheckpointEngine()
    {
        return new RetroPayCheckpointEngine(
            new List<IRetroPayCheckpointAdjuster>
            {
                new TieredRetroPayCheckpointAdjuster(
                    new List<double> { 1071.0, 3953.0, 8471.0 },
                    new List<double> { 0.88, 0.97, 1.15 }),
                new CapRetroPayCheckpointAdjuster(45174.0),
                new PercentRetroPayCheckpointAdjuster(-7.5),
            },
            new List<string>(),
            0.5,
            42375.8);
    }

    public double Run(RetroPayCheckpoint item)
    {
        var current = item.WeightedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RetroPayCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
