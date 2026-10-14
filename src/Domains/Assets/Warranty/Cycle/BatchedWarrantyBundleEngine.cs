namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched warranty bundle calculation.</summary>
public interface IBatchedWarrantyBundleStep
{
    string Describe();
    double Adjust(BatchedWarrantyBundle item, double valueValue);
}

public sealed record MinimumBatchedWarrantyBundleAdjuster(double Minimum) : IBatchedWarrantyBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedWarrantyBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBatchedWarrantyBundleAdjuster(double Step) : IBatchedWarrantyBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedWarrantyBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedWarrantyBundleAdjuster(Registry Flags, string Flag, double Percent) : IBatchedWarrantyBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedWarrantyBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedWarrantyBundleAmountOffsetAdjuster(double Weight) : IBatchedWarrantyBundleStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BatchedWarrantyBundle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the batched warranty bundle steps in order and keeps a trace of each.</summary>
public sealed class BatchedWarrantyBundleEngine(List<IBatchedWarrantyBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedWarrantyBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedWarrantyBundleEngine DefaultBatchedWarrantyBundleEngine(Registry flags)
    {
        return new BatchedWarrantyBundleEngine(
            new List<IBatchedWarrantyBundleStep>
            {
                new MinimumBatchedWarrantyBundleAdjuster(18.5),
                new RoundingBatchedWarrantyBundleAdjuster(0.1),
                new FlaggedBatchedWarrantyBundleAdjuster(flags, "batched-warranty-bundle-weighted", -3.1),
                new BatchedWarrantyBundleAmountOffsetAdjuster(1.97),
            },
            new List<string>(),
            1.7,
            94882.0);
    }

    public double Price(BatchedWarrantyBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedWarrantyBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
