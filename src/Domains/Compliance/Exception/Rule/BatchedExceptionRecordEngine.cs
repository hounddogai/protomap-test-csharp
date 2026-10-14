namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched exception record calculation.</summary>
public interface IBatchedExceptionRecordFactor
{
    string Describe();
    double Adjust(BatchedExceptionRecord item, double valueValue);
}

public sealed record RoundingBatchedExceptionRecordAdjuster(double Step) : IBatchedExceptionRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedExceptionRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedExceptionRecordAdjuster(double Cap) : IBatchedExceptionRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedExceptionRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedExceptionRecordAdjuster(List<double> Bounds, List<double> Rates) : IBatchedExceptionRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedExceptionRecord item, double valueValue)
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

/// <summary>Runs the batched exception record steps in order and keeps a trace of each.</summary>
public sealed class BatchedExceptionRecordEngine(List<IBatchedExceptionRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedExceptionRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedExceptionRecordEngine DefaultBatchedExceptionRecordEngine()
    {
        return new BatchedExceptionRecordEngine(
            new List<IBatchedExceptionRecordFactor>
            {
                new RoundingBatchedExceptionRecordAdjuster(0.05),
                new CapBatchedExceptionRecordAdjuster(43830.0),
                new TieredBatchedExceptionRecordAdjuster(
                    new List<double> { 1793.0, 6442.0 },
                    new List<double> { 0.87, 0.91 }),
            },
            new List<string>(),
            3.4,
            91639.3);
    }

    public double Evaluate(BatchedExceptionRecord item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedExceptionRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
