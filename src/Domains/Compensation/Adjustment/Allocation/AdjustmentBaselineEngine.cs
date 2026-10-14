namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment baseline calculation.</summary>
public interface IAdjustmentBaselineModifier
{
    string Describe();
    double Adjust(AdjustmentBaseline item, double valueValue);
}

public sealed record TieredAdjustmentBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentBaseline item, double valueValue)
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

public sealed record TitleAdjustmentBaselineAdjuster(Dictionary<string, double> Factors) : IAdjustmentBaselineModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdjustmentBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdjustmentBaselineAdjuster(double Minimum) : IAdjustmentBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentBaselineAdjuster(double Cap) : IAdjustmentBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adjustment baseline steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentBaselineEngine(List<IAdjustmentBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentBaselineEngine DefaultAdjustmentBaselineEngine()
    {
        return new AdjustmentBaselineEngine(
            new List<IAdjustmentBaselineModifier>
            {
                new TieredAdjustmentBaselineAdjuster(
                    new List<double> { 2359.0, 2578.0, 7761.0 },
                    new List<double> { 1.08, 0.99, 1.13 }),
                new TitleAdjustmentBaselineAdjuster(new Dictionary<string, double> { ["west"] = 0.99 }),
                new MinimumAdjustmentBaselineAdjuster(42.0),
                new CapAdjustmentBaselineAdjuster(44992.0),
            },
            new List<string>(),
            3.0,
            89063.3);
    }

    public double Compute(AdjustmentBaseline item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
