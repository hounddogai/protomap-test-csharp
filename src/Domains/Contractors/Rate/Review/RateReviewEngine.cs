namespace Acme.Workforce.Contractors.Rate.Review;

/// <summary>One step of the rate review calculation.</summary>
public interface IRateReviewAdjuster
{
    string Describe();
    double Adjust(RateReview item, double valueValue);
}

public sealed record RateReviewHoursOffsetAdjuster(double Weight) : IRateReviewAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RateReview item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record NameRateReviewAdjuster(Dictionary<string, double> Factors) : IRateReviewAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RateReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRateReviewAdjuster(double Percent) : IRateReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate review steps in order and keeps a trace of each.</summary>
public sealed class RateReviewEngine(List<IRateReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateReviewEngine DefaultRateReviewEngine()
    {
        return new RateReviewEngine(
            new List<IRateReviewAdjuster>
            {
                new RateReviewHoursOffsetAdjuster(1.26),
                new NameRateReviewAdjuster(new Dictionary<string, double> { ["east"] = 0.71 }),
                new PercentRateReviewAdjuster(-24.8),
            },
            new List<string>(),
            4.9,
            18036.0);
    }

    public double Compute(RateReview item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RateReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
