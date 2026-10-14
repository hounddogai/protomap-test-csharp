namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual vendor segment calculation.</summary>
public interface IManualVendorSegmentAdjuster
{
    string Describe();
    double Adjust(ManualVendorSegment item, double valueValue);
}

public sealed record RoundingManualVendorSegmentAdjuster(double Step) : IManualVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualVendorSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleManualVendorSegmentAdjuster(Dictionary<string, double> Factors) : IManualVendorSegmentAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ManualVendorSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualVendorSegmentAdjuster(List<double> Bounds, List<double> Rates) : IManualVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualVendorSegment item, double valueValue)
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

public sealed record CapManualVendorSegmentAdjuster(double Cap) : IManualVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualVendorSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualVendorSegmentPriorityOffsetAdjuster(double Weight) : IManualVendorSegmentAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ManualVendorSegment item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the manual vendor segment steps in order and keeps a trace of each.</summary>
public sealed class ManualVendorSegmentEngine(List<IManualVendorSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualVendorSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualVendorSegmentEngine DefaultManualVendorSegmentEngine()
    {
        return new ManualVendorSegmentEngine(
            new List<IManualVendorSegmentAdjuster>
            {
                new RoundingManualVendorSegmentAdjuster(0.25),
                new LocaleManualVendorSegmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.34, ["south"] = 0.86 }),
                new TieredManualVendorSegmentAdjuster(
                    new List<double> { 7645.0, 8888.0 },
                    new List<double> { 0.82, 1.13 }),
                new CapManualVendorSegmentAdjuster(43934.0),
                new ManualVendorSegmentPriorityOffsetAdjuster(1.51),
            },
            new List<string>(),
            4.5,
            57336.2);
    }

    public double Run(ManualVendorSegment item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualVendorSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
