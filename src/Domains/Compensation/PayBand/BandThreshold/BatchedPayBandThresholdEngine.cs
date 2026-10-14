namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched pay band threshold calculation.</summary>
public interface IBatchedPayBandThresholdAdjuster
{
    string Describe();
    double Adjust(BatchedPayBandThreshold item, double valueValue);
}

public sealed record RoundingBatchedPayBandThresholdAdjuster(double Step) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedPayBandThresholdAdjuster(double Percent) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedPayBandThresholdAdjuster(Registry Flags, string Flag, double Percent) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedPayBandThresholdAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
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

public sealed record BatchedPayBandThresholdMarginOffsetAdjuster(double Weight) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CountryBatchedPayBandThresholdAdjuster(Dictionary<string, double> Factors) : IBatchedPayBandThresholdAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedPayBandThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched pay band threshold steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayBandThresholdEngine(List<IBatchedPayBandThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayBandThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayBandThresholdEngine DefaultBatchedPayBandThresholdEngine(Registry flags)
    {
        return new BatchedPayBandThresholdEngine(
            new List<IBatchedPayBandThresholdAdjuster>
            {
                new RoundingBatchedPayBandThresholdAdjuster(0.1),
                new PercentBatchedPayBandThresholdAdjuster(-8.4),
                new FlaggedBatchedPayBandThresholdAdjuster(flags, "batched-pay-band-threshold-manual", -0.7),
                new TieredBatchedPayBandThresholdAdjuster(
                    new List<double> { 1153.0, 5165.0, 7671.0 },
                    new List<double> { 0.85, 1.06, 1.09 }),
                new BatchedPayBandThresholdMarginOffsetAdjuster(0.46),
                new CountryBatchedPayBandThresholdAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["east"] = 1.29 }),
            },
            new List<string>(),
            2.1,
            26003.5);
    }

    public double Compute(BatchedPayBandThreshold item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayBandThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
