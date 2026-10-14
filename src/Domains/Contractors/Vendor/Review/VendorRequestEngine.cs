namespace Acme.Workforce.Contractors.Vendor.Review;

/// <summary>One step of the vendor request calculation.</summary>
public interface IVendorRequestFactor
{
    string Describe();
    double Adjust(VendorRequest item, double valueValue);
}

public sealed record PercentVendorRequestAdjuster(double Percent) : IVendorRequestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVendorRequestAdjuster(double Cap) : IVendorRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredVendorRequestAdjuster(List<double> Bounds, List<double> Rates) : IVendorRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorRequest item, double valueValue)
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

public sealed record DepartmentVendorRequestAdjuster(Dictionary<string, double> Factors) : IVendorRequestFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(VendorRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record VendorRequestRevisionOffsetAdjuster(double Weight) : IVendorRequestFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(VendorRequest item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the vendor request steps in order and keeps a trace of each.</summary>
public sealed class VendorRequestEngine(List<IVendorRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorRequestEngine DefaultVendorRequestEngine()
    {
        return new VendorRequestEngine(
            new List<IVendorRequestFactor>
            {
                new PercentVendorRequestAdjuster(5.6),
                new CapVendorRequestAdjuster(34889.0),
                new TieredVendorRequestAdjuster(
                    new List<double> { 16.0, 142.0, 577.0, 6026.0 },
                    new List<double> { 0.91, 0.84, 1.02, 0.98 }),
                new DepartmentVendorRequestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.25, ["south"] = 0.72, ["east"] = 0.99 }),
                new VendorRequestRevisionOffsetAdjuster(0.88),
            },
            new List<string>(),
            4.8,
            76606.4);
    }

    public double Evaluate(VendorRequest item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<VendorRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
