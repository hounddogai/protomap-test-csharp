namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient vendor batch calculation.</summary>
public interface ILenientVendorBatchModifier
{
    string Describe();
    double Adjust(LenientVendorBatch item, double valueValue);
}

public sealed record RoundingLenientVendorBatchAdjuster(double Step) : ILenientVendorBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientVendorBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LenientVendorBatchFloorValueOffsetAdjuster(double Weight) : ILenientVendorBatchModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(LenientVendorBatch item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumLenientVendorBatchAdjuster(double Minimum) : ILenientVendorBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientVendorBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient vendor batch steps in order and keeps a trace of each.</summary>
public sealed class LenientVendorBatchEngine(List<ILenientVendorBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVendorBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVendorBatchEngine DefaultLenientVendorBatchEngine()
    {
        return new LenientVendorBatchEngine(
            new List<ILenientVendorBatchModifier>
            {
                new RoundingLenientVendorBatchAdjuster(0.05),
                new LenientVendorBatchFloorValueOffsetAdjuster(1.58),
                new MinimumLenientVendorBatchAdjuster(22.7),
            },
            new List<string>(),
            1.1,
            36509.5);
    }

    public double Run(LenientVendorBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientVendorBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
