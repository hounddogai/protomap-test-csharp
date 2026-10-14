namespace Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>One step of the mobility review calculation.</summary>
public interface IMobilityReviewAdjuster
{
    string Describe();
    double Adjust(MobilityReview item, double valueValue);
}

public sealed record TieredMobilityReviewAdjuster(List<double> Bounds, List<double> Rates) : IMobilityReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityReview item, double valueValue)
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

public sealed record MobilityReviewWeightOffsetAdjuster(double Weight) : IMobilityReviewAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(MobilityReview item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapMobilityReviewAdjuster(double Cap) : IMobilityReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the mobility review steps in order and keeps a trace of each.</summary>
public sealed class MobilityReviewEngine(List<IMobilityReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityReviewEngine DefaultMobilityReviewEngine()
    {
        return new MobilityReviewEngine(
            new List<IMobilityReviewAdjuster>
            {
                new TieredMobilityReviewAdjuster(
                    new List<double> { 1562.0, 5966.0, 7201.0 },
                    new List<double> { 0.87, 0.91, 0.96 }),
                new MobilityReviewWeightOffsetAdjuster(0.21),
                new CapMobilityReviewAdjuster(23095.0),
            },
            new List<string>(),
            2.8,
            13173.0);
    }

    public double Run(MobilityReview item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
