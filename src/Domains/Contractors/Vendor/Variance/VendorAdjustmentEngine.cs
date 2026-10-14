namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor adjustment calculation.</summary>
public interface IVendorAdjustmentAdjuster
{
    string Describe();
    double Adjust(VendorAdjustment item, double valueValue);
}

public sealed record PercentVendorAdjustmentAdjuster(double Percent) : IVendorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VendorAdjustmentCreatedAtOffsetAdjuster(double Weight) : IVendorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(VendorAdjustment item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingVendorAdjustmentAdjuster(double Step) : IVendorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor adjustment steps in order and keeps a trace of each.</summary>
public sealed class VendorAdjustmentEngine(List<IVendorAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorAdjustmentEngine DefaultVendorAdjustmentEngine()
    {
        return new VendorAdjustmentEngine(
            new List<IVendorAdjustmentAdjuster>
            {
                new PercentVendorAdjustmentAdjuster(-7.3),
                new VendorAdjustmentCreatedAtOffsetAdjuster(1.81),
                new RoundingVendorAdjustmentAdjuster(5.0),
            },
            new List<string>(),
            0.8,
            89921.3);
    }

    public double Evaluate(VendorAdjustment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<VendorAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
