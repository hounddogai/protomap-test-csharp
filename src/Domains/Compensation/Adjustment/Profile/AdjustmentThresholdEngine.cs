namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment threshold calculation.</summary>
public interface IAdjustmentThresholdFactor
{
    string Describe();
    double Adjust(AdjustmentThreshold item, double valueValue);
}

public sealed record RoundingAdjustmentThresholdAdjuster(double Step) : IAdjustmentThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AdjustmentThresholdLimitOffsetAdjuster(double Weight) : IAdjustmentThresholdFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdjustmentThreshold item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredAdjustmentThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentThreshold item, double valueValue)
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

/// <summary>Runs the adjustment threshold steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentThresholdEngine(List<IAdjustmentThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentThresholdEngine DefaultAdjustmentThresholdEngine()
    {
        return new AdjustmentThresholdEngine(
            new List<IAdjustmentThresholdFactor>
            {
                new RoundingAdjustmentThresholdAdjuster(0.05),
                new AdjustmentThresholdLimitOffsetAdjuster(0.98),
                new TieredAdjustmentThresholdAdjuster(
                    new List<double> { 429.0, 6156.0, 7636.0, 8002.0 },
                    new List<double> { 0.82, 1.01, 1.07, 1.13 }),
            },
            new List<string>(),
            4.0,
            97070.1);
    }

    public double Price(AdjustmentThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
