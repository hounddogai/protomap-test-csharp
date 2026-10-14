namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default adjustment estimate calculation.</summary>
public interface IDefaultAdjustmentEstimateModifier
{
    string Describe();
    double Adjust(DefaultAdjustmentEstimate item, double valueValue);
}

public sealed record RoundingDefaultAdjustmentEstimateAdjuster(double Step) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionDefaultAdjustmentEstimateAdjuster(Dictionary<string, double> Factors) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultAdjustmentEstimateAdjuster(double Cap) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultAdjustmentEstimateAdjuster(double Minimum) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultAdjustmentEstimateAdjuster(double Percent) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultAdjustmentEstimateAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAdjustmentEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAdjustmentEstimate item, double valueValue)
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

/// <summary>Runs the default adjustment estimate steps in order and keeps a trace of each.</summary>
public sealed class DefaultAdjustmentEstimateEngine(List<IDefaultAdjustmentEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAdjustmentEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAdjustmentEstimateEngine DefaultDefaultAdjustmentEstimateEngine()
    {
        return new DefaultAdjustmentEstimateEngine(
            new List<IDefaultAdjustmentEstimateModifier>
            {
                new RoundingDefaultAdjustmentEstimateAdjuster(0.1),
                new RegionDefaultAdjustmentEstimateAdjuster(new Dictionary<string, double> { ["retail"] = 0.73 }),
                new CapDefaultAdjustmentEstimateAdjuster(30719.0),
                new MinimumDefaultAdjustmentEstimateAdjuster(5.7),
                new PercentDefaultAdjustmentEstimateAdjuster(-0.2),
                new TieredDefaultAdjustmentEstimateAdjuster(
                    new List<double> { 6675.0, 8096.0 },
                    new List<double> { 1.12, 1.14 }),
            },
            new List<string>(),
            3.8,
            28748.2);
    }

    public double Compute(DefaultAdjustmentEstimate item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAdjustmentEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
