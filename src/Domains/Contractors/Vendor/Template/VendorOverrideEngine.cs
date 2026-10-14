namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor override calculation.</summary>
public interface IVendorOverrideFactor
{
    string Describe();
    double Adjust(VendorOverride item, double valueValue);
}

public sealed record MinimumVendorOverrideAdjuster(double Minimum) : IVendorOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredVendorOverrideAdjuster(List<double> Bounds, List<double> Rates) : IVendorOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorOverride item, double valueValue)
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

public sealed record CapVendorOverrideAdjuster(double Cap) : IVendorOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VendorOverrideCreatedAtOffsetAdjuster(double Weight) : IVendorOverrideFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(VendorOverride item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingVendorOverrideAdjuster(double Step) : IVendorOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor override steps in order and keeps a trace of each.</summary>
public sealed class VendorOverrideEngine(List<IVendorOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorOverrideEngine DefaultVendorOverrideEngine()
    {
        return new VendorOverrideEngine(
            new List<IVendorOverrideFactor>
            {
                new MinimumVendorOverrideAdjuster(49.4),
                new TieredVendorOverrideAdjuster(
                    new List<double> { 2687.0, 2819.0, 3808.0, 5207.0 },
                    new List<double> { 1.01, 1.06, 1.15, 0.91 }),
                new CapVendorOverrideAdjuster(23552.0),
                new VendorOverrideCreatedAtOffsetAdjuster(1.71),
                new RoundingVendorOverrideAdjuster(0.25),
            },
            new List<string>(),
            2.8,
            20781.6);
    }

    public double Compute(VendorOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<VendorOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
