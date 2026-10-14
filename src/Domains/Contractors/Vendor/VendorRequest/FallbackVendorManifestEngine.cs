namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback vendor manifest calculation.</summary>
public interface IFallbackVendorManifestAdjuster
{
    string Describe();
    double Adjust(FallbackVendorManifest item, double valueValue);
}

public sealed record FlaggedFallbackVendorManifestAdjuster(Registry Flags, string Flag, double Percent) : IFallbackVendorManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackVendorManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackVendorManifestAdjuster(double Cap) : IFallbackVendorManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackVendorManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackVendorManifestAdjuster(List<double> Bounds, List<double> Rates) : IFallbackVendorManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackVendorManifest item, double valueValue)
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

/// <summary>Runs the fallback vendor manifest steps in order and keeps a trace of each.</summary>
public sealed class FallbackVendorManifestEngine(List<IFallbackVendorManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackVendorManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackVendorManifestEngine DefaultFallbackVendorManifestEngine(Registry flags)
    {
        return new FallbackVendorManifestEngine(
            new List<IFallbackVendorManifestAdjuster>
            {
                new FlaggedFallbackVendorManifestAdjuster(flags, "fallback-vendor-manifest-global", 9.5),
                new CapFallbackVendorManifestAdjuster(8912.0),
                new TieredFallbackVendorManifestAdjuster(
                    new List<double> { 2934.0, 2962.0, 2995.0, 6605.0 },
                    new List<double> { 0.89, 0.86, 0.96, 1.12 }),
            },
            new List<string>(),
            2.3,
            59796.0);
    }

    public double Evaluate(FallbackVendorManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackVendorManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
