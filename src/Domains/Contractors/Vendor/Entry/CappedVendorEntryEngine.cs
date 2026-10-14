namespace Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>One step of the capped vendor entry calculation.</summary>
public interface ICappedVendorEntryAdjuster
{
    string Describe();
    double Adjust(CappedVendorEntry item, double valueValue);
}

public sealed record MinimumCappedVendorEntryAdjuster(double Minimum) : ICappedVendorEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedVendorEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedVendorEntryAdjuster(double Percent) : ICappedVendorEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedVendorEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedVendorEntryAdjuster(List<double> Bounds, List<double> Rates) : ICappedVendorEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedVendorEntry item, double valueValue)
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

public sealed record CappedVendorEntryLeaveDaysOffsetAdjuster(double Weight) : ICappedVendorEntryAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(CappedVendorEntry item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the capped vendor entry steps in order and keeps a trace of each.</summary>
public sealed class CappedVendorEntryEngine(List<ICappedVendorEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedVendorEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedVendorEntryEngine DefaultCappedVendorEntryEngine()
    {
        return new CappedVendorEntryEngine(
            new List<ICappedVendorEntryAdjuster>
            {
                new MinimumCappedVendorEntryAdjuster(14.8),
                new PercentCappedVendorEntryAdjuster(6.9),
                new TieredCappedVendorEntryAdjuster(
                    new List<double> { 788.0, 7699.0, 7737.0, 8201.0 },
                    new List<double> { 1.12, 0.81, 0.91, 1.04 }),
                new CappedVendorEntryLeaveDaysOffsetAdjuster(1.43),
            },
            new List<string>(),
            1.3,
            98932.1);
    }

    public double Run(CappedVendorEntry item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedVendorEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
