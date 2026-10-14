namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive desk batch calculation.</summary>
public interface IAdaptiveDeskBatchFactor
{
    string Describe();
    double Adjust(AdaptiveDeskBatch item, double valueValue);
}

public sealed record SegmentAdaptiveDeskBatchAdjuster(Dictionary<string, double> Factors) : IAdaptiveDeskBatchFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AdaptiveDeskBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveDeskBatchAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveDeskBatchFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveDeskBatch item, double valueValue)
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

public sealed record AdaptiveDeskBatchRetriesOffsetAdjuster(double Weight) : IAdaptiveDeskBatchFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AdaptiveDeskBatch item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedAdaptiveDeskBatchAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveDeskBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveDeskBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveDeskBatchAdjuster(double Cap) : IAdaptiveDeskBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveDeskBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive desk batch steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveDeskBatchEngine(List<IAdaptiveDeskBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveDeskBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveDeskBatchEngine DefaultAdaptiveDeskBatchEngine(Registry flags)
    {
        return new AdaptiveDeskBatchEngine(
            new List<IAdaptiveDeskBatchFactor>
            {
                new SegmentAdaptiveDeskBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.82, ["retail"] = 1.03 }),
                new TieredAdaptiveDeskBatchAdjuster(
                    new List<double> { 595.0, 6055.0 },
                    new List<double> { 0.99, 0.95 }),
                new AdaptiveDeskBatchRetriesOffsetAdjuster(0.05),
                new FlaggedAdaptiveDeskBatchAdjuster(flags, "adaptive-desk-batch-adaptive", -1.6),
                new CapAdaptiveDeskBatchAdjuster(5178.0),
            },
            new List<string>(),
            2.8,
            92844.0);
    }

    public double Run(AdaptiveDeskBatch item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveDeskBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
