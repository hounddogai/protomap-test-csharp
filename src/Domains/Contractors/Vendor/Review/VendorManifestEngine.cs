namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor manifest calculation.</summary>
public interface IVendorManifestModifier
{
    string Describe();
    double Adjust(VendorManifest item, double valueValue);
}

public sealed record CapVendorManifestAdjuster(double Cap) : IVendorManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVendorManifestAdjuster(double Step) : IVendorManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedVendorManifestAdjuster(Registry Flags, string Flag, double Percent) : IVendorManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelVendorManifestAdjuster(Dictionary<string, double> Factors) : IVendorManifestModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(VendorManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the vendor manifest steps in order and keeps a trace of each.</summary>
public sealed class VendorManifestEngine(List<IVendorManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorManifestEngine DefaultVendorManifestEngine(Registry flags)
    {
        return new VendorManifestEngine(
            new List<IVendorManifestModifier>
            {
                new CapVendorManifestAdjuster(21979.0),
                new RoundingVendorManifestAdjuster(1.0),
                new FlaggedVendorManifestAdjuster(flags, "vendor-manifest-fallback", 5.9),
                new ChannelVendorManifestAdjuster(new Dictionary<string, double> { ["south"] = 0.87 }),
            },
            new List<string>(),
            3.3,
            71303.5);
    }

    public double Run(VendorManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<VendorManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
