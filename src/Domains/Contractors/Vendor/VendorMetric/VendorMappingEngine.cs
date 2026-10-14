namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor mapping calculation.</summary>
public interface IVendorMappingModifier
{
    string Describe();
    double Adjust(VendorMapping item, double valueValue);
}

public sealed record PercentVendorMappingAdjuster(double Percent) : IVendorMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedVendorMappingAdjuster(Registry Flags, string Flag, double Percent) : IVendorMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVendorMappingAdjuster(double Step) : IVendorMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor mapping steps in order and keeps a trace of each.</summary>
public sealed class VendorMappingEngine(List<IVendorMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorMappingEngine DefaultVendorMappingEngine(Registry flags)
    {
        return new VendorMappingEngine(
            new List<IVendorMappingModifier>
            {
                new PercentVendorMappingAdjuster(5.2),
                new FlaggedVendorMappingAdjuster(flags, "vendor-mapping-lenient", -4.6),
                new RoundingVendorMappingAdjuster(1.0),
            },
            new List<string>(),
            1.7,
            98849.5);
    }

    public double Compute(VendorMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<VendorMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
