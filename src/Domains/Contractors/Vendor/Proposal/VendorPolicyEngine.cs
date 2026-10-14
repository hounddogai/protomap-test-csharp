namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor policy calculation.</summary>
public interface IVendorPolicyAdjuster
{
    string Describe();
    double Adjust(VendorPolicy item, double valueValue);
}

public sealed record CapVendorPolicyAdjuster(double Cap) : IVendorPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedVendorPolicyAdjuster(Registry Flags, string Flag, double Percent) : IVendorPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVendorPolicyAdjuster(List<double> Bounds, List<double> Rates) : IVendorPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorPolicy item, double valueValue)
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

public sealed record PercentVendorPolicyAdjuster(double Percent) : IVendorPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the vendor policy steps in order and keeps a trace of each.</summary>
public sealed class VendorPolicyEngine(List<IVendorPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorPolicyEngine DefaultVendorPolicyEngine(Registry flags)
    {
        return new VendorPolicyEngine(
            new List<IVendorPolicyAdjuster>
            {
                new CapVendorPolicyAdjuster(42330.0),
                new FlaggedVendorPolicyAdjuster(flags, "vendor-policy-legacy", -7.2),
                new TieredVendorPolicyAdjuster(
                    new List<double> { 3477.0, 5188.0, 8049.0 },
                    new List<double> { 1.1, 0.83, 1.05 }),
                new PercentVendorPolicyAdjuster(-11.2),
            },
            new List<string>(),
            1.8,
            46782.6);
    }

    public double Price(VendorPolicy item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<VendorPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
