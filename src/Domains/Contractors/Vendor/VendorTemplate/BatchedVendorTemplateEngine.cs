namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched vendor template calculation.</summary>
public interface IBatchedVendorTemplateStep
{
    string Describe();
    double Adjust(BatchedVendorTemplate item, double valueValue);
}

public sealed record TitleBatchedVendorTemplateAdjuster(Dictionary<string, double> Factors) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BatchedVendorTemplateRevisionOffsetAdjuster(double Weight) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentBatchedVendorTemplateAdjuster(double Percent) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedVendorTemplateAdjuster(Registry Flags, string Flag, double Percent) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedVendorTemplateAdjuster(List<double> Bounds, List<double> Rates) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
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

public sealed record CapBatchedVendorTemplateAdjuster(double Cap) : IBatchedVendorTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedVendorTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched vendor template steps in order and keeps a trace of each.</summary>
public sealed class BatchedVendorTemplateEngine(List<IBatchedVendorTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedVendorTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedVendorTemplateEngine DefaultBatchedVendorTemplateEngine(Registry flags)
    {
        return new BatchedVendorTemplateEngine(
            new List<IBatchedVendorTemplateStep>
            {
                new TitleBatchedVendorTemplateAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.27, ["east"] = 0.97 }),
                new BatchedVendorTemplateRevisionOffsetAdjuster(1.18),
                new PercentBatchedVendorTemplateAdjuster(-15.8),
                new FlaggedBatchedVendorTemplateAdjuster(flags, "batched-vendor-template-global", 1.4),
                new TieredBatchedVendorTemplateAdjuster(
                    new List<double> { 4574.0, 6522.0, 7508.0, 8402.0 },
                    new List<double> { 0.87, 0.98, 1.1, 1.07 }),
                new CapBatchedVendorTemplateAdjuster(15580.0),
            },
            new List<string>(),
            4.8,
            95579.0);
    }

    public double Evaluate(BatchedVendorTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedVendorTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
