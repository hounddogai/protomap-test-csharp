namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient vendor baseline calculation.</summary>
public interface ILenientVendorBaselineFactor
{
    string Describe();
    double Adjust(LenientVendorBaseline item, double valueValue);
}

public sealed record CapLenientVendorBaselineAdjuster(double Cap) : ILenientVendorBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientVendorBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLenientVendorBaselineAdjuster(Registry Flags, string Flag, double Percent) : ILenientVendorBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientVendorBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientVendorBaselineAdjuster(double Percent) : ILenientVendorBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientVendorBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient vendor baseline steps in order and keeps a trace of each.</summary>
public sealed class LenientVendorBaselineEngine(List<ILenientVendorBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVendorBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVendorBaselineEngine DefaultLenientVendorBaselineEngine(Registry flags)
    {
        return new LenientVendorBaselineEngine(
            new List<ILenientVendorBaselineFactor>
            {
                new CapLenientVendorBaselineAdjuster(20888.0),
                new FlaggedLenientVendorBaselineAdjuster(flags, "lenient-vendor-baseline-primary", -9.5),
                new PercentLenientVendorBaselineAdjuster(21.5),
            },
            new List<string>(),
            3.1,
            89214.2);
    }

    public double Run(LenientVendorBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientVendorBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
