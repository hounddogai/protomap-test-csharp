namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor ledger calculation.</summary>
public interface IVendorLedgerModifier
{
    string Describe();
    double Adjust(VendorLedger item, double valueValue);
}

public sealed record CapVendorLedgerAdjuster(double Cap) : IVendorLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVendorLedgerAdjuster(double Step) : IVendorLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record VendorLedgerMarginOffsetAdjuster(double Weight) : IVendorLedgerModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(VendorLedger item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumVendorLedgerAdjuster(double Minimum) : IVendorLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentVendorLedgerAdjuster(double Percent) : IVendorLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the vendor ledger steps in order and keeps a trace of each.</summary>
public sealed class VendorLedgerEngine(List<IVendorLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorLedgerEngine DefaultVendorLedgerEngine()
    {
        return new VendorLedgerEngine(
            new List<IVendorLedgerModifier>
            {
                new CapVendorLedgerAdjuster(10998.0),
                new RoundingVendorLedgerAdjuster(0.5),
                new VendorLedgerMarginOffsetAdjuster(0.4),
                new MinimumVendorLedgerAdjuster(29.0),
                new PercentVendorLedgerAdjuster(11.7),
            },
            new List<string>(),
            1.6,
            31159.8);
    }

    public double Run(VendorLedger item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VendorLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
