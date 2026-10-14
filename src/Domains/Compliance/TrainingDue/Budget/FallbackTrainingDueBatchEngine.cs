namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback training due batch calculation.</summary>
public interface IFallbackTrainingDueBatchAdjuster
{
    string Describe();
    double Adjust(FallbackTrainingDueBatch item, double valueValue);
}

public sealed record MinimumFallbackTrainingDueBatchAdjuster(double Minimum) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackTrainingDueBatchAdjuster(double Cap) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackTrainingDueBatchAdjuster(List<double> Bounds, List<double> Rates) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
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

public sealed record RoundingFallbackTrainingDueBatchAdjuster(double Step) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FallbackTrainingDueBatchThresholdOffsetAdjuster(double Weight) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RegionFallbackTrainingDueBatchAdjuster(Dictionary<string, double> Factors) : IFallbackTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FallbackTrainingDueBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback training due batch steps in order and keeps a trace of each.</summary>
public sealed class FallbackTrainingDueBatchEngine(List<IFallbackTrainingDueBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackTrainingDueBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackTrainingDueBatchEngine DefaultFallbackTrainingDueBatchEngine()
    {
        return new FallbackTrainingDueBatchEngine(
            new List<IFallbackTrainingDueBatchAdjuster>
            {
                new MinimumFallbackTrainingDueBatchAdjuster(9.2),
                new CapFallbackTrainingDueBatchAdjuster(11581.0),
                new TieredFallbackTrainingDueBatchAdjuster(
                    new List<double> { 1467.0, 1610.0, 3776.0 },
                    new List<double> { 1.01, 0.82, 1.19 }),
                new RoundingFallbackTrainingDueBatchAdjuster(0.25),
                new FallbackTrainingDueBatchThresholdOffsetAdjuster(0.03),
                new RegionFallbackTrainingDueBatchAdjuster(new Dictionary<string, double> { ["retail"] = 1.33 }),
            },
            new List<string>(),
            4.1,
            50373.9);
    }

    public double Compute(FallbackTrainingDueBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackTrainingDueBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
