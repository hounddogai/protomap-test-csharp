namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy span adjustment calculation.</summary>
public interface ILegacySpanAdjustmentFactor
{
    string Describe();
    double Adjust(LegacySpanAdjustment item, double valueValue);
}

public sealed record RoundingLegacySpanAdjustmentAdjuster(double Step) : ILegacySpanAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacySpanAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameLegacySpanAdjustmentAdjuster(Dictionary<string, double> Factors) : ILegacySpanAdjustmentFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacySpanAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacySpanAdjustmentDiscountOffsetAdjuster(double Weight) : ILegacySpanAdjustmentFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LegacySpanAdjustment item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the legacy span adjustment steps in order and keeps a trace of each.</summary>
public sealed class LegacySpanAdjustmentEngine(List<ILegacySpanAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacySpanAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacySpanAdjustmentEngine DefaultLegacySpanAdjustmentEngine()
    {
        return new LegacySpanAdjustmentEngine(
            new List<ILegacySpanAdjustmentFactor>
            {
                new RoundingLegacySpanAdjustmentAdjuster(0.5),
                new NameLegacySpanAdjustmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.96 }),
                new LegacySpanAdjustmentDiscountOffsetAdjuster(1.91),
            },
            new List<string>(),
            3.2,
            95944.5);
    }

    public double Evaluate(LegacySpanAdjustment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacySpanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
