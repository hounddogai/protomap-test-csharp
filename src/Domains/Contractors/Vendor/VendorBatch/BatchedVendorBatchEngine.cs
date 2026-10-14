namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched vendor batch calculation.</summary>
public interface IBatchedVendorBatchModifier
{
    string Describe();
    double Adjust(BatchedVendorBatch item, double valueValue);
}

public sealed record FlaggedBatchedVendorBatchAdjuster(Registry Flags, string Flag, double Percent) : IBatchedVendorBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedVendorBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBatchedVendorBatchAdjuster(double Percent) : IBatchedVendorBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedVendorBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelBatchedVendorBatchAdjuster(Dictionary<string, double> Factors) : IBatchedVendorBatchModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BatchedVendorBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched vendor batch steps in order and keeps a trace of each.</summary>
public sealed class BatchedVendorBatchEngine(List<IBatchedVendorBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedVendorBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedVendorBatchEngine DefaultBatchedVendorBatchEngine(Registry flags)
    {
        return new BatchedVendorBatchEngine(
            new List<IBatchedVendorBatchModifier>
            {
                new FlaggedBatchedVendorBatchAdjuster(flags, "batched-vendor-batch-pending", 4.6),
                new PercentBatchedVendorBatchAdjuster(-23.4),
                new LabelBatchedVendorBatchAdjuster(new Dictionary<string, double> { ["east"] = 1.1 }),
            },
            new List<string>(),
            4.5,
            53652.2);
    }

    public double Evaluate(BatchedVendorBatch item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedVendorBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
