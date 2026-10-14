namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor threshold calculation.</summary>
public interface IVendorThresholdModifier
{
    string Describe();
    double Adjust(VendorThreshold item, double valueValue);
}

public sealed record MinimumVendorThresholdAdjuster(double Minimum) : IVendorThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapVendorThresholdAdjuster(double Cap) : IVendorThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedVendorThresholdAdjuster(Registry Flags, string Flag, double Percent) : IVendorThresholdModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVendorThresholdAdjuster(double Step) : IVendorThresholdModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredVendorThresholdAdjuster(List<double> Bounds, List<double> Rates) : IVendorThresholdModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorThreshold item, double valueValue)
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

public sealed record UnitVendorThresholdAdjuster(Dictionary<string, double> Factors) : IVendorThresholdModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(VendorThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the vendor threshold steps in order and keeps a trace of each.</summary>
public sealed class VendorThresholdEngine(List<IVendorThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorThresholdEngine DefaultVendorThresholdEngine(Registry flags)
    {
        return new VendorThresholdEngine(
            new List<IVendorThresholdModifier>
            {
                new MinimumVendorThresholdAdjuster(31.2),
                new CapVendorThresholdAdjuster(43518.0),
                new FlaggedVendorThresholdAdjuster(flags, "vendor-threshold-fallback", 0.8),
                new RoundingVendorThresholdAdjuster(5.0),
                new TieredVendorThresholdAdjuster(new List<double> { 458.0, 2927.0 }, new List<double> { 0.9, 0.84 }),
                new UnitVendorThresholdAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.36, ["retail"] = 0.75 }),
            },
            new List<string>(),
            2.0,
            11420.5);
    }

    public double Price(VendorThreshold item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<VendorThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
