namespace Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>One step of the vendor bundle calculation.</summary>
public interface IVendorBundleFactor
{
    string Describe();
    double Adjust(VendorBundle item, double valueValue);
}

public sealed record TieredVendorBundleAdjuster(List<double> Bounds, List<double> Rates) : IVendorBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorBundle item, double valueValue)
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

public sealed record PercentVendorBundleAdjuster(double Percent) : IVendorBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVendorBundleAdjuster(double Cap) : IVendorBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VendorBundleHoursOffsetAdjuster(double Weight) : IVendorBundleFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(VendorBundle item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumVendorBundleAdjuster(double Minimum) : IVendorBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleVendorBundleAdjuster(Dictionary<string, double> Factors) : IVendorBundleFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(VendorBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the vendor bundle steps in order and keeps a trace of each.</summary>
public sealed class VendorBundleEngine(List<IVendorBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorBundleEngine DefaultVendorBundleEngine()
    {
        return new VendorBundleEngine(
            new List<IVendorBundleFactor>
            {
                new TieredVendorBundleAdjuster(new List<double> { 666.0, 5206.0 }, new List<double> { 1.08, 1.08 }),
                new PercentVendorBundleAdjuster(6.2),
                new CapVendorBundleAdjuster(35656.0),
                new VendorBundleHoursOffsetAdjuster(0.7),
                new MinimumVendorBundleAdjuster(35.8),
                new TitleVendorBundleAdjuster(new Dictionary<string, double> { ["retail"] = 1.22 }),
            },
            new List<string>(),
            5.0,
            26453.7);
    }

    public double Run(VendorBundle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<VendorBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
