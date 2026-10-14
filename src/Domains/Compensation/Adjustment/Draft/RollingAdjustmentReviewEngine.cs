namespace Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>One step of the rolling adjustment review calculation.</summary>
public interface IRollingAdjustmentReviewFactor
{
    string Describe();
    double Adjust(RollingAdjustmentReview item, double valueValue);
}

public sealed record RollingAdjustmentReviewRateOffsetAdjuster(double Weight) : IRollingAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RollingAdjustmentReview item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record CapRollingAdjustmentReviewAdjuster(double Cap) : IRollingAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAdjustmentReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamRollingAdjustmentReviewAdjuster(Dictionary<string, double> Factors) : IRollingAdjustmentReviewFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RollingAdjustmentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRollingAdjustmentReviewAdjuster(List<double> Bounds, List<double> Rates) : IRollingAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAdjustmentReview item, double valueValue)
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

public sealed record PercentRollingAdjustmentReviewAdjuster(double Percent) : IRollingAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAdjustmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling adjustment review steps in order and keeps a trace of each.</summary>
public sealed class RollingAdjustmentReviewEngine(List<IRollingAdjustmentReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAdjustmentReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAdjustmentReviewEngine DefaultRollingAdjustmentReviewEngine()
    {
        return new RollingAdjustmentReviewEngine(
            new List<IRollingAdjustmentReviewFactor>
            {
                new RollingAdjustmentReviewRateOffsetAdjuster(1.16),
                new CapRollingAdjustmentReviewAdjuster(36773.0),
                new TeamRollingAdjustmentReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.24, ["central"] = 1.21, ["north"] = 1.08 }),
                new TieredRollingAdjustmentReviewAdjuster(
                    new List<double> { 4172.0, 8453.0 },
                    new List<double> { 1.03, 0.89 }),
                new PercentRollingAdjustmentReviewAdjuster(-20.3),
            },
            new List<string>(),
            2.3,
            71463.5);
    }

    public double Compute(RollingAdjustmentReview item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAdjustmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
