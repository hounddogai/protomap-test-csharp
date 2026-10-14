namespace Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>One step of the occupancy review calculation.</summary>
public interface IOccupancyReviewFactor
{
    string Describe();
    double Adjust(OccupancyReview item, double valueValue);
}

public sealed record PercentOccupancyReviewAdjuster(double Percent) : IOccupancyReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record OccupancyReviewStartsAtOffsetAdjuster(double Weight) : IOccupancyReviewFactor
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(OccupancyReview item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CategoryOccupancyReviewAdjuster(Dictionary<string, double> Factors) : IOccupancyReviewFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(OccupancyReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredOccupancyReviewAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyReview item, double valueValue)
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

/// <summary>Runs the occupancy review steps in order and keeps a trace of each.</summary>
public sealed class OccupancyReviewEngine(List<IOccupancyReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyReviewEngine DefaultOccupancyReviewEngine()
    {
        return new OccupancyReviewEngine(
            new List<IOccupancyReviewFactor>
            {
                new PercentOccupancyReviewAdjuster(18.0),
                new OccupancyReviewStartsAtOffsetAdjuster(0.35),
                new CategoryOccupancyReviewAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["north"] = 0.83, ["west"] = 1.06 }),
                new TieredOccupancyReviewAdjuster(
                    new List<double> { 1090.0, 1342.0, 5805.0, 6468.0 },
                    new List<double> { 1.11, 0.89, 0.81, 0.94 }),
            },
            new List<string>(),
            3.6,
            9477.7);
    }

    public double Evaluate(OccupancyReview item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
