namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor template calculation.</summary>
public interface IVendorTemplateAdjuster
{
    string Describe();
    double Adjust(VendorTemplate item, double valueValue);
}

public sealed record CapVendorTemplateAdjuster(double Cap) : IVendorTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedVendorTemplateAdjuster(Registry Flags, string Flag, double Percent) : IVendorTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVendorTemplateAdjuster(double Step) : IVendorTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor template steps in order and keeps a trace of each.</summary>
public sealed class VendorTemplateEngine(List<IVendorTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorTemplateEngine DefaultVendorTemplateEngine(Registry flags)
    {
        return new VendorTemplateEngine(
            new List<IVendorTemplateAdjuster>
            {
                new CapVendorTemplateAdjuster(23527.0),
                new FlaggedVendorTemplateAdjuster(flags, "vendor-template-rolling", 6.9),
                new RoundingVendorTemplateAdjuster(5.0),
            },
            new List<string>(),
            3.5,
            31818.6);
    }

    public double Compute(VendorTemplate item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<VendorTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
