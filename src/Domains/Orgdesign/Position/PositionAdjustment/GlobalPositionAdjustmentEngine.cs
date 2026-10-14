namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>One step of the global position adjustment calculation.</summary>
public interface IGlobalPositionAdjustmentModifier
{
    string Describe();
    double Adjust(GlobalPositionAdjustment item, double valueValue);
}

public sealed record TieredGlobalPositionAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPositionAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPositionAdjustment item, double valueValue)
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

public sealed record SegmentGlobalPositionAdjustmentAdjuster(Dictionary<string, double> Factors) : IGlobalPositionAdjustmentModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(GlobalPositionAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGlobalPositionAdjustmentAdjuster(double Cap) : IGlobalPositionAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPositionAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global position adjustment steps in order and keeps a trace of each.</summary>
public sealed class GlobalPositionAdjustmentEngine(List<IGlobalPositionAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPositionAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPositionAdjustmentEngine DefaultGlobalPositionAdjustmentEngine()
    {
        return new GlobalPositionAdjustmentEngine(
            new List<IGlobalPositionAdjustmentModifier>
            {
                new TieredGlobalPositionAdjustmentAdjuster(
                    new List<double> { 1371.0, 3188.0, 7477.0, 7659.0 },
                    new List<double> { 1.14, 0.82, 1.16, 0.97 }),
                new SegmentGlobalPositionAdjustmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.98 }),
                new CapGlobalPositionAdjustmentAdjuster(22379.0),
            },
            new List<string>(),
            1.8,
            21752.1);
    }

    public double Compute(GlobalPositionAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPositionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
