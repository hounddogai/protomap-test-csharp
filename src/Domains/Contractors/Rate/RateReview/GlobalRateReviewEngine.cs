namespace Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>One step of the global rate review calculation.</summary>
public interface IGlobalRateReviewFactor
{
    string Describe();
    double Adjust(GlobalRateReview item, double valueValue);
}

public sealed record PercentGlobalRateReviewAdjuster(double Percent) : IGlobalRateReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalRateReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalRateReviewAttemptsOffsetAdjuster(double Weight) : IGlobalRateReviewFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(GlobalRateReview item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumGlobalRateReviewAdjuster(double Minimum) : IGlobalRateReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalRateReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalRateReviewAdjuster(double Cap) : IGlobalRateReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalRateReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredGlobalRateReviewAdjuster(List<double> Bounds, List<double> Rates) : IGlobalRateReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalRateReview item, double valueValue)
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

/// <summary>Runs the global rate review steps in order and keeps a trace of each.</summary>
public sealed class GlobalRateReviewEngine(List<IGlobalRateReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalRateReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalRateReviewEngine DefaultGlobalRateReviewEngine()
    {
        return new GlobalRateReviewEngine(
            new List<IGlobalRateReviewFactor>
            {
                new PercentGlobalRateReviewAdjuster(17.6),
                new GlobalRateReviewAttemptsOffsetAdjuster(0.26),
                new MinimumGlobalRateReviewAdjuster(13.0),
                new CapGlobalRateReviewAdjuster(10034.0),
                new TieredGlobalRateReviewAdjuster(
                    new List<double> { 2871.0, 4737.0, 7199.0, 7448.0 },
                    new List<double> { 1.07, 0.96, 1.11, 0.91 }),
            },
            new List<string>(),
            0.5,
            24518.5);
    }

    public double Price(GlobalRateReview item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalRateReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
