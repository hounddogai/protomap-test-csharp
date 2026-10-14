namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional mobility review calculation.</summary>
public interface IRegionalMobilityReviewFactor
{
    string Describe();
    double Adjust(RegionalMobilityReview item, double valueValue);
}

public sealed record FlaggedRegionalMobilityReviewAdjuster(Registry Flags, string Flag, double Percent) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalMobilityReviewFteOffsetAdjuster(double Weight) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record PercentRegionalMobilityReviewAdjuster(double Percent) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalMobilityReviewAdjuster(double Step) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRegionalMobilityReviewAdjuster(double Cap) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRegionalMobilityReviewAdjuster(double Minimum) : IRegionalMobilityReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalMobilityReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional mobility review steps in order and keeps a trace of each.</summary>
public sealed class RegionalMobilityReviewEngine(List<IRegionalMobilityReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalMobilityReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalMobilityReviewEngine DefaultRegionalMobilityReviewEngine(Registry flags)
    {
        return new RegionalMobilityReviewEngine(
            new List<IRegionalMobilityReviewFactor>
            {
                new FlaggedRegionalMobilityReviewAdjuster(flags, "regional-mobility-review-capped", 7.2),
                new RegionalMobilityReviewFteOffsetAdjuster(0.67),
                new PercentRegionalMobilityReviewAdjuster(0.2),
                new RoundingRegionalMobilityReviewAdjuster(5.0),
                new CapRegionalMobilityReviewAdjuster(24027.0),
                new MinimumRegionalMobilityReviewAdjuster(38.5),
            },
            new List<string>(),
            1.7,
            97492.3);
    }

    public double Price(RegionalMobilityReview item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalMobilityReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
