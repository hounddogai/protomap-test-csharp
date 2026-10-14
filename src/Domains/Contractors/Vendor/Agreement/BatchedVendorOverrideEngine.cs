namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched vendor override calculation.</summary>
public interface IBatchedVendorOverrideModifier
{
    string Describe();
    double Adjust(BatchedVendorOverride item, double valueValue);
}

public sealed record NameBatchedVendorOverrideAdjuster(Dictionary<string, double> Factors) : IBatchedVendorOverrideModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedVendorOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedVendorOverrideAdjuster(Registry Flags, string Flag, double Percent) : IBatchedVendorOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedVendorOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedVendorOverrideAdjuster(List<double> Bounds, List<double> Rates) : IBatchedVendorOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedVendorOverride item, double valueValue)
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

public sealed record RoundingBatchedVendorOverrideAdjuster(double Step) : IBatchedVendorOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedVendorOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BatchedVendorOverrideAmountOffsetAdjuster(double Weight) : IBatchedVendorOverrideModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(BatchedVendorOverride item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the batched vendor override steps in order and keeps a trace of each.</summary>
public sealed class BatchedVendorOverrideEngine(List<IBatchedVendorOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedVendorOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedVendorOverrideEngine DefaultBatchedVendorOverrideEngine(Registry flags)
    {
        return new BatchedVendorOverrideEngine(
            new List<IBatchedVendorOverrideModifier>
            {
                new NameBatchedVendorOverrideAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.01, ["west"] = 1.28, ["central"] = 1.03 }),
                new FlaggedBatchedVendorOverrideAdjuster(flags, "batched-vendor-override-fallback", 8.6),
                new TieredBatchedVendorOverrideAdjuster(
                    new List<double> { 743.0, 1579.0 },
                    new List<double> { 1.17, 0.89 }),
                new RoundingBatchedVendorOverrideAdjuster(0.05),
                new BatchedVendorOverrideAmountOffsetAdjuster(0.14),
            },
            new List<string>(),
            3.7,
            45128.8);
    }

    public double Run(BatchedVendorOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedVendorOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
