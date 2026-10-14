namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched pay run variance calculation.</summary>
public interface IBatchedPayRunVarianceModifier
{
    string Describe();
    double Adjust(BatchedPayRunVariance item, double valueValue);
}

public sealed record TieredBatchedPayRunVarianceAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPayRunVariance item, double valueValue)
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

public sealed record RoundingBatchedPayRunVarianceAdjuster(double Step) : IBatchedPayRunVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayRunVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedPayRunVarianceAdjuster(double Percent) : IBatchedPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPayRunVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedPayRunVarianceAdjuster(double Cap) : IBatchedPayRunVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPayRunVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched pay run variance steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayRunVarianceEngine(List<IBatchedPayRunVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayRunVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayRunVarianceEngine DefaultBatchedPayRunVarianceEngine()
    {
        return new BatchedPayRunVarianceEngine(
            new List<IBatchedPayRunVarianceModifier>
            {
                new TieredBatchedPayRunVarianceAdjuster(
                    new List<double> { 838.0, 7328.0 },
                    new List<double> { 1.02, 0.99 }),
                new RoundingBatchedPayRunVarianceAdjuster(1.0),
                new PercentBatchedPayRunVarianceAdjuster(8.9),
                new CapBatchedPayRunVarianceAdjuster(5296.0),
            },
            new List<string>(),
            1.8,
            61442.3);
    }

    public double Run(BatchedPayRunVariance item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayRunVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
