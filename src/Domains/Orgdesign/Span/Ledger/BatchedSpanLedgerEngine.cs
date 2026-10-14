namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched span ledger calculation.</summary>
public interface IBatchedSpanLedgerStep
{
    string Describe();
    double Adjust(BatchedSpanLedger item, double valueValue);
}

public sealed record BatchedSpanLedgerQuantityOffsetAdjuster(double Weight) : IBatchedSpanLedgerStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(BatchedSpanLedger item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RoundingBatchedSpanLedgerAdjuster(double Step) : IBatchedSpanLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedSpanLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedSpanLedgerAdjuster(double Cap) : IBatchedSpanLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedSpanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceBatchedSpanLedgerAdjuster(Dictionary<string, double> Factors) : IBatchedSpanLedgerStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(BatchedSpanLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedSpanLedgerAdjuster(double Percent) : IBatchedSpanLedgerStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedSpanLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched span ledger steps in order and keeps a trace of each.</summary>
public sealed class BatchedSpanLedgerEngine(List<IBatchedSpanLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSpanLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSpanLedgerEngine DefaultBatchedSpanLedgerEngine()
    {
        return new BatchedSpanLedgerEngine(
            new List<IBatchedSpanLedgerStep>
            {
                new BatchedSpanLedgerQuantityOffsetAdjuster(0.43),
                new RoundingBatchedSpanLedgerAdjuster(0.25),
                new CapBatchedSpanLedgerAdjuster(1982.0),
                new SourceBatchedSpanLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.34, ["north"] = 1.12, ["west"] = 0.83 }),
                new PercentBatchedSpanLedgerAdjuster(11.9),
            },
            new List<string>(),
            4.0,
            64766.1);
    }

    public double Run(BatchedSpanLedger item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSpanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
