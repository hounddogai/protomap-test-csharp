namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>One step of the vendor draft calculation.</summary>
public interface IVendorDraftFactor
{
    string Describe();
    double Adjust(VendorDraft item, double valueValue);
}

public sealed record TieredVendorDraftAdjuster(List<double> Bounds, List<double> Rates) : IVendorDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorDraft item, double valueValue)
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

public sealed record CountryVendorDraftAdjuster(Dictionary<string, double> Factors) : IVendorDraftFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(VendorDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumVendorDraftAdjuster(double Minimum) : IVendorDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentVendorDraftAdjuster(double Percent) : IVendorDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VendorDraftMaximumOffsetAdjuster(double Weight) : IVendorDraftFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(VendorDraft item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapVendorDraftAdjuster(double Cap) : IVendorDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the vendor draft steps in order and keeps a trace of each.</summary>
public sealed class VendorDraftEngine(List<IVendorDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorDraftEngine DefaultVendorDraftEngine()
    {
        return new VendorDraftEngine(
            new List<IVendorDraftFactor>
            {
                new TieredVendorDraftAdjuster(
                    new List<double> { 5005.0, 5707.0, 5746.0, 8558.0 },
                    new List<double> { 1.04, 0.91, 0.88, 1.05 }),
                new CountryVendorDraftAdjuster(new Dictionary<string, double> { ["central"] = 0.8 }),
                new MinimumVendorDraftAdjuster(49.3),
                new PercentVendorDraftAdjuster(16.4),
                new VendorDraftMaximumOffsetAdjuster(1.91),
                new CapVendorDraftAdjuster(42368.0),
            },
            new List<string>(),
            1.2,
            39766.1);
    }

    public double Price(VendorDraft item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VendorDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
