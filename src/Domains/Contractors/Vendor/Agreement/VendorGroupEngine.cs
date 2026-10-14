namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor group calculation.</summary>
public interface IVendorGroupFactor
{
    string Describe();
    double Adjust(VendorGroup item, double valueValue);
}

public sealed record TieredVendorGroupAdjuster(List<double> Bounds, List<double> Rates) : IVendorGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VendorGroup item, double valueValue)
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

public sealed record MinimumVendorGroupAdjuster(double Minimum) : IVendorGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapVendorGroupAdjuster(double Cap) : IVendorGroupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVendorGroupAdjuster(double Step) : IVendorGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor group steps in order and keeps a trace of each.</summary>
public sealed class VendorGroupEngine(List<IVendorGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorGroupEngine DefaultVendorGroupEngine()
    {
        return new VendorGroupEngine(
            new List<IVendorGroupFactor>
            {
                new TieredVendorGroupAdjuster(
                    new List<double> { 5243.0, 7577.0, 9254.0 },
                    new List<double> { 0.84, 0.84, 0.92 }),
                new MinimumVendorGroupAdjuster(1.1),
                new CapVendorGroupAdjuster(22334.0),
                new RoundingVendorGroupAdjuster(5.0),
            },
            new List<string>(),
            1.1,
            7336.5);
    }

    public double Evaluate(VendorGroup item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<VendorGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
