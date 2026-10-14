namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered warranty profile calculation.</summary>
public interface ITieredWarrantyProfileStep
{
    string Describe();
    double Adjust(TieredWarrantyProfile item, double valueValue);
}

public sealed record RoundingTieredWarrantyProfileAdjuster(double Step) : ITieredWarrantyProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredWarrantyProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentTieredWarrantyProfileAdjuster(Dictionary<string, double> Factors) : ITieredWarrantyProfileStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredWarrantyProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWarrantyProfileRateOffsetAdjuster(double Weight) : ITieredWarrantyProfileStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(TieredWarrantyProfile item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the tiered warranty profile steps in order and keeps a trace of each.</summary>
public sealed class TieredWarrantyProfileEngine(List<ITieredWarrantyProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredWarrantyProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredWarrantyProfileEngine DefaultTieredWarrantyProfileEngine()
    {
        return new TieredWarrantyProfileEngine(
            new List<ITieredWarrantyProfileStep>
            {
                new RoundingTieredWarrantyProfileAdjuster(0.1),
                new SegmentTieredWarrantyProfileAdjuster(new Dictionary<string, double> { ["north"] = 0.75 }),
                new TieredWarrantyProfileRateOffsetAdjuster(1.39),
            },
            new List<string>(),
            1.4,
            93510.3);
    }

    public double Compute(TieredWarrantyProfile item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredWarrantyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
