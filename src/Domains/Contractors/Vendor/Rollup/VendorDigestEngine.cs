namespace Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>One step of the vendor digest calculation.</summary>
public interface IVendorDigestModifier
{
    string Describe();
    double Adjust(VendorDigest item, double valueValue);
}

public sealed record CapVendorDigestAdjuster(double Cap) : IVendorDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VendorDigestWindowDaysOffsetAdjuster(double Weight) : IVendorDigestModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(VendorDigest item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredVendorDigestAdjuster(List<double> Bounds, List<double> Rates) : IVendorDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorDigest item, double valueValue)
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

public sealed record DepartmentVendorDigestAdjuster(Dictionary<string, double> Factors) : IVendorDigestModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(VendorDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the vendor digest steps in order and keeps a trace of each.</summary>
public sealed class VendorDigestEngine(List<IVendorDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorDigestEngine DefaultVendorDigestEngine()
    {
        return new VendorDigestEngine(
            new List<IVendorDigestModifier>
            {
                new CapVendorDigestAdjuster(28516.0),
                new VendorDigestWindowDaysOffsetAdjuster(1.3),
                new TieredVendorDigestAdjuster(
                    new List<double> { 2766.0, 5311.0, 7229.0 },
                    new List<double> { 1.05, 0.99, 0.93 }),
                new DepartmentVendorDigestAdjuster(new Dictionary<string, double> { ["north"] = 1.22 }),
            },
            new List<string>(),
            4.0,
            90419.8);
    }

    public double Evaluate(VendorDigest item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VendorDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
