namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor baseline calculation.</summary>
public interface IVendorBaselineModifier
{
    string Describe();
    double Adjust(VendorBaseline item, double valueValue);
}

public sealed record FlaggedVendorBaselineAdjuster(Registry Flags, string Flag, double Percent) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVendorBaselineAdjuster(List<double> Bounds, List<double> Rates) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorBaseline item, double valueValue)
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

public sealed record VendorBaselineFteOffsetAdjuster(double Weight) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(VendorBaseline item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumVendorBaselineAdjuster(double Minimum) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentVendorBaselineAdjuster(double Percent) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVendorBaselineAdjuster(double Cap) : IVendorBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the vendor baseline steps in order and keeps a trace of each.</summary>
public sealed class VendorBaselineEngine(List<IVendorBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorBaselineEngine DefaultVendorBaselineEngine(Registry flags)
    {
        return new VendorBaselineEngine(
            new List<IVendorBaselineModifier>
            {
                new FlaggedVendorBaselineAdjuster(flags, "vendor-baseline-pending", 5.1),
                new TieredVendorBaselineAdjuster(
                    new List<double> { 1477.0, 6141.0 },
                    new List<double> { 0.91, 0.92 }),
                new VendorBaselineFteOffsetAdjuster(0.42),
                new MinimumVendorBaselineAdjuster(17.1),
                new PercentVendorBaselineAdjuster(11.5),
                new CapVendorBaselineAdjuster(32351.0),
            },
            new List<string>(),
            2.0,
            96130.9);
    }

    public double Price(VendorBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<VendorBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
