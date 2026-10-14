namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor record calculation.</summary>
public interface IVendorRecordModifier
{
    string Describe();
    double Adjust(VendorRecord item, double valueValue);
}

public sealed record MinimumVendorRecordAdjuster(double Minimum) : IVendorRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedVendorRecordAdjuster(Registry Flags, string Flag, double Percent) : IVendorRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryVendorRecordAdjuster(Dictionary<string, double> Factors) : IVendorRecordModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentVendorRecordAdjuster(double Percent) : IVendorRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVendorRecordAdjuster(double Cap) : IVendorRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVendorRecordAdjuster(double Step) : IVendorRecordModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor record steps in order and keeps a trace of each.</summary>
public sealed class VendorRecordEngine(List<IVendorRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorRecordEngine DefaultVendorRecordEngine(Registry flags)
    {
        return new VendorRecordEngine(
            new List<IVendorRecordModifier>
            {
                new MinimumVendorRecordAdjuster(36.6),
                new FlaggedVendorRecordAdjuster(flags, "vendor-record-manual", -8.7),
                new CountryVendorRecordAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.87, ["central"] = 1.23 }),
                new PercentVendorRecordAdjuster(-23.5),
                new CapVendorRecordAdjuster(33761.0),
                new RoundingVendorRecordAdjuster(5.0),
            },
            new List<string>(),
            2.2,
            72534.3);
    }

    public double Run(VendorRecord item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<VendorRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
