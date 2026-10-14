namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient vendor reservation calculation.</summary>
public interface ILenientVendorReservationAdjuster
{
    string Describe();
    double Adjust(LenientVendorReservation item, double valueValue);
}

public sealed record TieredLenientVendorReservationAdjuster(List<double> Bounds, List<double> Rates) : ILenientVendorReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientVendorReservation item, double valueValue)
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

public sealed record LenientVendorReservationBatchSizeOffsetAdjuster(double Weight) : ILenientVendorReservationAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(LenientVendorReservation item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record LocaleLenientVendorReservationAdjuster(Dictionary<string, double> Factors) : ILenientVendorReservationAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LenientVendorReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientVendorReservationAdjuster(double Cap) : ILenientVendorReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientVendorReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientVendorReservationAdjuster(double Step) : ILenientVendorReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientVendorReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient vendor reservation steps in order and keeps a trace of each.</summary>
public sealed class LenientVendorReservationEngine(List<ILenientVendorReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVendorReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVendorReservationEngine DefaultLenientVendorReservationEngine()
    {
        return new LenientVendorReservationEngine(
            new List<ILenientVendorReservationAdjuster>
            {
                new TieredLenientVendorReservationAdjuster(
                    new List<double> { 3343.0, 8443.0 },
                    new List<double> { 0.85, 1.06 }),
                new LenientVendorReservationBatchSizeOffsetAdjuster(0.49),
                new LocaleLenientVendorReservationAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["retail"] = 0.75 }),
                new CapLenientVendorReservationAdjuster(17093.0),
                new RoundingLenientVendorReservationAdjuster(0.1),
            },
            new List<string>(),
            2.9,
            98915.9);
    }

    public double Compute(LenientVendorReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientVendorReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
