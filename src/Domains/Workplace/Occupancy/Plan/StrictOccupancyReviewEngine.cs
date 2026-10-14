namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict occupancy review calculation.</summary>
public interface IStrictOccupancyReviewAdjuster
{
    string Describe();
    double Adjust(StrictOccupancyReview item, double valueValue);
}

public sealed record MinimumStrictOccupancyReviewAdjuster(double Minimum) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictOccupancyReviewAdjuster(double Cap) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictOccupancyReviewRateOffsetAdjuster(double Weight) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingStrictOccupancyReviewAdjuster(double Step) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStrictOccupancyReviewAdjuster(List<double> Bounds, List<double> Rates) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
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

public sealed record TitleStrictOccupancyReviewAdjuster(Dictionary<string, double> Factors) : IStrictOccupancyReviewAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(StrictOccupancyReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict occupancy review steps in order and keeps a trace of each.</summary>
public sealed class StrictOccupancyReviewEngine(List<IStrictOccupancyReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictOccupancyReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictOccupancyReviewEngine DefaultStrictOccupancyReviewEngine()
    {
        return new StrictOccupancyReviewEngine(
            new List<IStrictOccupancyReviewAdjuster>
            {
                new MinimumStrictOccupancyReviewAdjuster(40.1),
                new CapStrictOccupancyReviewAdjuster(36187.0),
                new StrictOccupancyReviewRateOffsetAdjuster(0.76),
                new RoundingStrictOccupancyReviewAdjuster(0.05),
                new TieredStrictOccupancyReviewAdjuster(
                    new List<double> { 881.0, 4549.0 },
                    new List<double> { 0.91, 1.0 }),
                new TitleStrictOccupancyReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.88, ["east"] = 1.01 }),
            },
            new List<string>(),
            2.5,
            69092.2);
    }

    public double Run(StrictOccupancyReview item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictOccupancyReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
