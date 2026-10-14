namespace Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>One step of the bonus pool review calculation.</summary>
public interface IBonusPoolReviewFactor
{
    string Describe();
    double Adjust(BonusPoolReview item, double valueValue);
}

public sealed record PercentBonusPoolReviewAdjuster(double Percent) : IBonusPoolReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBonusPoolReviewAdjuster(double Cap) : IBonusPoolReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleBonusPoolReviewAdjuster(Dictionary<string, double> Factors) : IBonusPoolReviewFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BonusPoolReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BonusPoolReviewScoreOffsetAdjuster(double Weight) : IBonusPoolReviewFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(BonusPoolReview item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredBonusPoolReviewAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolReview item, double valueValue)
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

/// <summary>Runs the bonus pool review steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolReviewEngine(List<IBonusPoolReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolReviewEngine DefaultBonusPoolReviewEngine()
    {
        return new BonusPoolReviewEngine(
            new List<IBonusPoolReviewFactor>
            {
                new PercentBonusPoolReviewAdjuster(19.6),
                new CapBonusPoolReviewAdjuster(38377.0),
                new TitleBonusPoolReviewAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.91, ["east"] = 0.99 }),
                new BonusPoolReviewScoreOffsetAdjuster(1.44),
                new TieredBonusPoolReviewAdjuster(
                    new List<double> { 1239.0, 1970.0, 2052.0, 6334.0 },
                    new List<double> { 1.2, 1.15, 1.05, 0.93 }),
            },
            new List<string>(),
            3.0,
            65138.4);
    }

    public double Evaluate(BonusPoolReview item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
