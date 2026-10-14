namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal adjustment review calculation.</summary>
public interface ISeasonalAdjustmentReviewFactor
{
    string Describe();
    double Adjust(SeasonalAdjustmentReview item, double valueValue);
}

public sealed record TitleSeasonalAdjustmentReviewAdjuster(Dictionary<string, double> Factors) : ISeasonalAdjustmentReviewFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalAdjustmentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalAdjustmentReviewAdjuster(double Percent) : ISeasonalAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalAdjustmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalAdjustmentReviewAdjuster(double Cap) : ISeasonalAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAdjustmentReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalAdjustmentReviewBatchSizeOffsetAdjuster(double Weight) : ISeasonalAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(SeasonalAdjustmentReview item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredSeasonalAdjustmentReviewAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAdjustmentReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAdjustmentReview item, double valueValue)
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

/// <summary>Runs the seasonal adjustment review steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAdjustmentReviewEngine(List<ISeasonalAdjustmentReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAdjustmentReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAdjustmentReviewEngine DefaultSeasonalAdjustmentReviewEngine()
    {
        return new SeasonalAdjustmentReviewEngine(
            new List<ISeasonalAdjustmentReviewFactor>
            {
                new TitleSeasonalAdjustmentReviewAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.15, ["west"] = 1.22, ["online"] = 0.96 }),
                new PercentSeasonalAdjustmentReviewAdjuster(-23.5),
                new CapSeasonalAdjustmentReviewAdjuster(48886.0),
                new SeasonalAdjustmentReviewBatchSizeOffsetAdjuster(0.3),
                new TieredSeasonalAdjustmentReviewAdjuster(
                    new List<double> { 523.0, 3974.0, 8468.0 },
                    new List<double> { 1.11, 0.96, 0.9 }),
            },
            new List<string>(),
            3.8,
            45059.0);
    }

    public double Price(SeasonalAdjustmentReview item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAdjustmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
