namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual span batch calculation.</summary>
public interface IManualSpanBatchFactor
{
    string Describe();
    double Adjust(ManualSpanBatch item, double valueValue);
}

public sealed record ManualSpanBatchRatioOffsetAdjuster(double Weight) : IManualSpanBatchFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(ManualSpanBatch item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record ChannelManualSpanBatchAdjuster(Dictionary<string, double> Factors) : IManualSpanBatchFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ManualSpanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapManualSpanBatchAdjuster(double Cap) : IManualSpanBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualSpanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredManualSpanBatchAdjuster(List<double> Bounds, List<double> Rates) : IManualSpanBatchFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualSpanBatch item, double valueValue)
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

/// <summary>Runs the manual span batch steps in order and keeps a trace of each.</summary>
public sealed class ManualSpanBatchEngine(List<IManualSpanBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSpanBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSpanBatchEngine DefaultManualSpanBatchEngine()
    {
        return new ManualSpanBatchEngine(
            new List<IManualSpanBatchFactor>
            {
                new ManualSpanBatchRatioOffsetAdjuster(0.43),
                new ChannelManualSpanBatchAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.71, ["east"] = 1.16 }),
                new CapManualSpanBatchAdjuster(22955.0),
                new TieredManualSpanBatchAdjuster(
                    new List<double> { 2898.0, 6866.0 },
                    new List<double> { 1.18, 0.89 }),
            },
            new List<string>(),
            1.3,
            61300.0);
    }

    public double Compute(ManualSpanBatch item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSpanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
