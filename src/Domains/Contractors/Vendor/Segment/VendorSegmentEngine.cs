namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor segment calculation.</summary>
public interface IVendorSegmentModifier
{
    string Describe();
    double Adjust(VendorSegment item, double valueValue);
}

public sealed record VendorSegmentCeilingOffsetAdjuster(double Weight) : IVendorSegmentModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(VendorSegment item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record TieredVendorSegmentAdjuster(List<double> Bounds, List<double> Rates) : IVendorSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorSegment item, double valueValue)
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

public sealed record PercentVendorSegmentAdjuster(double Percent) : IVendorSegmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleVendorSegmentAdjuster(Dictionary<string, double> Factors) : IVendorSegmentModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(VendorSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingVendorSegmentAdjuster(double Step) : IVendorSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedVendorSegmentAdjuster(Registry Flags, string Flag, double Percent) : IVendorSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the vendor segment steps in order and keeps a trace of each.</summary>
public sealed class VendorSegmentEngine(List<IVendorSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorSegmentEngine DefaultVendorSegmentEngine(Registry flags)
    {
        return new VendorSegmentEngine(
            new List<IVendorSegmentModifier>
            {
                new VendorSegmentCeilingOffsetAdjuster(1.57),
                new TieredVendorSegmentAdjuster(
                    new List<double> { 1164.0, 1792.0, 6238.0, 6552.0 },
                    new List<double> { 0.95, 0.88, 1.11, 1.17 }),
                new PercentVendorSegmentAdjuster(10.3),
                new TitleVendorSegmentAdjuster(new Dictionary<string, double> { ["east"] = 1.19 }),
                new RoundingVendorSegmentAdjuster(0.1),
                new FlaggedVendorSegmentAdjuster(flags, "vendor-segment-regional", -0.4),
            },
            new List<string>(),
            4.7,
            25055.8);
    }

    public double Compute(VendorSegment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VendorSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
