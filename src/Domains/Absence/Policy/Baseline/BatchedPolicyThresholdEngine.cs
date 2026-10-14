namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched policy threshold calculation.</summary>
public interface IBatchedPolicyThresholdFactor
{
    string Describe();
    double Adjust(BatchedPolicyThreshold item, double valueValue);
}

public sealed record RoundingBatchedPolicyThresholdAdjuster(double Step) : IBatchedPolicyThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPolicyThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBatchedPolicyThresholdAdjuster(double Minimum) : IBatchedPolicyThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedPolicyThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBatchedPolicyThresholdAdjuster(double Percent) : IBatchedPolicyThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPolicyThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedPolicyThresholdAdjuster(double Cap) : IBatchedPolicyThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPolicyThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedPolicyThresholdAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPolicyThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPolicyThreshold item, double valueValue)
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

/// <summary>Runs the batched policy threshold steps in order and keeps a trace of each.</summary>
public sealed class BatchedPolicyThresholdEngine(List<IBatchedPolicyThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPolicyThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPolicyThresholdEngine DefaultBatchedPolicyThresholdEngine()
    {
        return new BatchedPolicyThresholdEngine(
            new List<IBatchedPolicyThresholdFactor>
            {
                new RoundingBatchedPolicyThresholdAdjuster(0.25),
                new MinimumBatchedPolicyThresholdAdjuster(3.9),
                new PercentBatchedPolicyThresholdAdjuster(-21.9),
                new CapBatchedPolicyThresholdAdjuster(37497.0),
                new TieredBatchedPolicyThresholdAdjuster(
                    new List<double> { 718.0, 3406.0, 6269.0 },
                    new List<double> { 0.89, 1.15, 1.12 }),
            },
            new List<string>(),
            2.0,
            74726.1);
    }

    public double Price(BatchedPolicyThreshold item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPolicyThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
