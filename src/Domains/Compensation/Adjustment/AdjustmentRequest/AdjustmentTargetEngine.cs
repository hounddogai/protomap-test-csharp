namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>One step of the adjustment target calculation.</summary>
public interface IAdjustmentTargetAdjuster
{
    string Describe();
    double Adjust(AdjustmentTarget item, double valueValue);
}

public sealed record PercentAdjustmentTargetAdjuster(double Percent) : IAdjustmentTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdjustmentTargetAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentTarget item, double valueValue)
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

public sealed record AdjustmentTargetCreatedAtOffsetAdjuster(double Weight) : IAdjustmentTargetAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(AdjustmentTarget item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the adjustment target steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentTargetEngine(List<IAdjustmentTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentTargetEngine DefaultAdjustmentTargetEngine()
    {
        return new AdjustmentTargetEngine(
            new List<IAdjustmentTargetAdjuster>
            {
                new PercentAdjustmentTargetAdjuster(6.9),
                new TieredAdjustmentTargetAdjuster(
                    new List<double> { 339.0, 2148.0, 5340.0, 5511.0 },
                    new List<double> { 0.89, 1.05, 1.03, 1.14 }),
                new AdjustmentTargetCreatedAtOffsetAdjuster(1.94),
            },
            new List<string>(),
            0.8,
            48961.8);
    }

    public double Evaluate(AdjustmentTarget item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
