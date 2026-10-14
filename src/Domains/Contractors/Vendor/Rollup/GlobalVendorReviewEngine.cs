namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global vendor review calculation.</summary>
public interface IGlobalVendorReviewAdjuster
{
    string Describe();
    double Adjust(GlobalVendorReview item, double valueValue);
}

public sealed record MinimumGlobalVendorReviewAdjuster(double Minimum) : IGlobalVendorReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalVendorReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalVendorReviewBatchSizeOffsetAdjuster(double Weight) : IGlobalVendorReviewAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(GlobalVendorReview item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapGlobalVendorReviewAdjuster(double Cap) : IGlobalVendorReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalVendorReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGlobalVendorReviewAdjuster(List<double> Bounds, List<double> Rates) : IGlobalVendorReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalVendorReview item, double valueValue)
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

/// <summary>Runs the global vendor review steps in order and keeps a trace of each.</summary>
public sealed class GlobalVendorReviewEngine(List<IGlobalVendorReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalVendorReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalVendorReviewEngine DefaultGlobalVendorReviewEngine()
    {
        return new GlobalVendorReviewEngine(
            new List<IGlobalVendorReviewAdjuster>
            {
                new MinimumGlobalVendorReviewAdjuster(36.9),
                new GlobalVendorReviewBatchSizeOffsetAdjuster(1.42),
                new CapGlobalVendorReviewAdjuster(2863.0),
                new TieredGlobalVendorReviewAdjuster(
                    new List<double> { 565.0, 722.0, 1272.0, 8109.0 },
                    new List<double> { 1.09, 1.16, 1.14, 1.12 }),
            },
            new List<string>(),
            1.7,
            32663.1);
    }

    public double Price(GlobalVendorReview item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalVendorReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
