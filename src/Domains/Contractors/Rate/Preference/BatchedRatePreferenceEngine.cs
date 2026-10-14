namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched rate preference calculation.</summary>
public interface IBatchedRatePreferenceStep
{
    string Describe();
    double Adjust(BatchedRatePreference item, double valueValue);
}

public sealed record SegmentBatchedRatePreferenceAdjuster(Dictionary<string, double> Factors) : IBatchedRatePreferenceStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BatchedRatePreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBatchedRatePreferenceAdjuster(double Step) : IBatchedRatePreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedRatePreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBatchedRatePreferenceAdjuster(List<double> Bounds, List<double> Rates) : IBatchedRatePreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedRatePreference item, double valueValue)
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

/// <summary>Runs the batched rate preference steps in order and keeps a trace of each.</summary>
public sealed class BatchedRatePreferenceEngine(List<IBatchedRatePreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRatePreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRatePreferenceEngine DefaultBatchedRatePreferenceEngine()
    {
        return new BatchedRatePreferenceEngine(
            new List<IBatchedRatePreferenceStep>
            {
                new SegmentBatchedRatePreferenceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.95, ["west"] = 0.78, ["north"] = 1.35 }),
                new RoundingBatchedRatePreferenceAdjuster(0.05),
                new TieredBatchedRatePreferenceAdjuster(
                    new List<double> { 2967.0, 3677.0, 3772.0 },
                    new List<double> { 1.01, 1.03, 0.99 }),
            },
            new List<string>(),
            1.7,
            1905.2);
    }

    public double Compute(BatchedRatePreference item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRatePreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
