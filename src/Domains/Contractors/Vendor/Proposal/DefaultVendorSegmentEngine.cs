namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default vendor segment calculation.</summary>
public interface IDefaultVendorSegmentAdjuster
{
    string Describe();
    double Adjust(DefaultVendorSegment item, double valueValue);
}

public sealed record TieredDefaultVendorSegmentAdjuster(List<double> Bounds, List<double> Rates) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
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

public sealed record MinimumDefaultVendorSegmentAdjuster(double Minimum) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultVendorSegmentAdjuster(Registry Flags, string Flag, double Percent) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultVendorSegmentAdjuster(double Percent) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultVendorSegmentFloorValueOffsetAdjuster(double Weight) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapDefaultVendorSegmentAdjuster(double Cap) : IDefaultVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultVendorSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default vendor segment steps in order and keeps a trace of each.</summary>
public sealed class DefaultVendorSegmentEngine(List<IDefaultVendorSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultVendorSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultVendorSegmentEngine DefaultDefaultVendorSegmentEngine(Registry flags)
    {
        return new DefaultVendorSegmentEngine(
            new List<IDefaultVendorSegmentAdjuster>
            {
                new TieredDefaultVendorSegmentAdjuster(
                    new List<double> { 1240.0, 3122.0 },
                    new List<double> { 1.2, 0.88 }),
                new MinimumDefaultVendorSegmentAdjuster(32.5),
                new FlaggedDefaultVendorSegmentAdjuster(flags, "default-vendor-segment-rolling", 7.0),
                new PercentDefaultVendorSegmentAdjuster(-2.6),
                new DefaultVendorSegmentFloorValueOffsetAdjuster(0.93),
                new CapDefaultVendorSegmentAdjuster(2222.0),
            },
            new List<string>(),
            4.5,
            68531.6);
    }

    public double Run(DefaultVendorSegment item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultVendorSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
