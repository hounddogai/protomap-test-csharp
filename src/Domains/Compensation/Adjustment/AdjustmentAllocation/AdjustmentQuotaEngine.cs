namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment quota calculation.</summary>
public interface IAdjustmentQuotaModifier
{
    string Describe();
    double Adjust(AdjustmentQuota item, double valueValue);
}

public sealed record TieredAdjustmentQuotaAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentQuota item, double valueValue)
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

public sealed record PercentAdjustmentQuotaAdjuster(double Percent) : IAdjustmentQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdjustmentQuotaDiscountOffsetAdjuster(double Weight) : IAdjustmentQuotaModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(AdjustmentQuota item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record MinimumAdjustmentQuotaAdjuster(double Minimum) : IAdjustmentQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adjustment quota steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentQuotaEngine(List<IAdjustmentQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentQuotaEngine DefaultAdjustmentQuotaEngine()
    {
        return new AdjustmentQuotaEngine(
            new List<IAdjustmentQuotaModifier>
            {
                new TieredAdjustmentQuotaAdjuster(
                    new List<double> { 7185.0, 9243.0, 9440.0 },
                    new List<double> { 0.81, 1.11, 0.83 }),
                new PercentAdjustmentQuotaAdjuster(6.6),
                new AdjustmentQuotaDiscountOffsetAdjuster(0.8),
                new MinimumAdjustmentQuotaAdjuster(10.2),
            },
            new List<string>(),
            1.2,
            46964.5);
    }

    public double Price(AdjustmentQuota item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
