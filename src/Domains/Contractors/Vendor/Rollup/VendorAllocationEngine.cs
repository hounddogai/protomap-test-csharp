namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the vendor allocation calculation.</summary>
public interface IVendorAllocationAdjuster
{
    string Describe();
    double Adjust(VendorAllocation item, double valueValue);
}

public sealed record MinimumVendorAllocationAdjuster(double Minimum) : IVendorAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedVendorAllocationAdjuster(Registry Flags, string Flag, double Percent) : IVendorAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelVendorAllocationAdjuster(Dictionary<string, double> Factors) : IVendorAllocationAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(VendorAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the vendor allocation steps in order and keeps a trace of each.</summary>
public sealed class VendorAllocationEngine(List<IVendorAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorAllocationEngine DefaultVendorAllocationEngine(Registry flags)
    {
        return new VendorAllocationEngine(
            new List<IVendorAllocationAdjuster>
            {
                new MinimumVendorAllocationAdjuster(26.4),
                new FlaggedVendorAllocationAdjuster(flags, "vendor-allocation-seasonal", 1.0),
                new ChannelVendorAllocationAdjuster(new Dictionary<string, double> { ["north"] = 1.32 }),
            },
            new List<string>(),
            4.4,
            80848.3);
    }

    public double Compute(VendorAllocation item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<VendorAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
