namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty segment calculation.</summary>
public interface IWarrantySegmentFactor
{
    string Describe();
    double Adjust(WarrantySegment item, double valueValue);
}

public sealed record ChannelWarrantySegmentAdjuster(Dictionary<string, double> Factors) : IWarrantySegmentFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WarrantySegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WarrantySegmentCapacityOffsetAdjuster(double Weight) : IWarrantySegmentFactor
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(WarrantySegment item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record CapWarrantySegmentAdjuster(double Cap) : IWarrantySegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWarrantySegmentAdjuster(double Step) : IWarrantySegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the warranty segment steps in order and keeps a trace of each.</summary>
public sealed class WarrantySegmentEngine(List<IWarrantySegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantySegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantySegmentEngine DefaultWarrantySegmentEngine()
    {
        return new WarrantySegmentEngine(
            new List<IWarrantySegmentFactor>
            {
                new ChannelWarrantySegmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["north"] = 0.78 }),
                new WarrantySegmentCapacityOffsetAdjuster(0.72),
                new CapWarrantySegmentAdjuster(30050.0),
                new RoundingWarrantySegmentAdjuster(0.5),
            },
            new List<string>(),
            4.6,
            18695.7);
    }

    public double Run(WarrantySegment item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
