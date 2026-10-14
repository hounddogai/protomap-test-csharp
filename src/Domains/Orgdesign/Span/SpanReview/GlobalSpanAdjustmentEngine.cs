namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>One step of the global span adjustment calculation.</summary>
public interface IGlobalSpanAdjustmentFactor
{
    string Describe();
    double Adjust(GlobalSpanAdjustment item, double valueValue);
}

public sealed record MinimumGlobalSpanAdjustmentAdjuster(double Minimum) : IGlobalSpanAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSpanAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalSpanAdjustmentFloorValueOffsetAdjuster(double Weight) : IGlobalSpanAdjustmentFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(GlobalSpanAdjustment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredGlobalSpanAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalSpanAdjustmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalSpanAdjustment item, double valueValue)
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

/// <summary>Runs the global span adjustment steps in order and keeps a trace of each.</summary>
public sealed class GlobalSpanAdjustmentEngine(List<IGlobalSpanAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSpanAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSpanAdjustmentEngine DefaultGlobalSpanAdjustmentEngine()
    {
        return new GlobalSpanAdjustmentEngine(
            new List<IGlobalSpanAdjustmentFactor>
            {
                new MinimumGlobalSpanAdjustmentAdjuster(33.7),
                new GlobalSpanAdjustmentFloorValueOffsetAdjuster(0.41),
                new TieredGlobalSpanAdjustmentAdjuster(
                    new List<double> { 3847.0, 9328.0 },
                    new List<double> { 0.85, 0.98 }),
            },
            new List<string>(),
            0.2,
            81351.9);
    }

    public double Price(GlobalSpanAdjustment item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSpanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
