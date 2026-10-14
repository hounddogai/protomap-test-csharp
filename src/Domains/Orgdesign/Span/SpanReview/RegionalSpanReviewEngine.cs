namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional span review calculation.</summary>
public interface IRegionalSpanReviewFactor
{
    string Describe();
    double Adjust(RegionalSpanReview item, double valueValue);
}

public sealed record PercentRegionalSpanReviewAdjuster(double Percent) : IRegionalSpanReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSpanReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelRegionalSpanReviewAdjuster(Dictionary<string, double> Factors) : IRegionalSpanReviewFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalSpanReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalSpanReviewAdjuster(double Step) : IRegionalSpanReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSpanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRegionalSpanReviewAdjuster(double Cap) : IRegionalSpanReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSpanReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRegionalSpanReviewAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSpanReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSpanReview item, double valueValue)
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

/// <summary>Runs the regional span review steps in order and keeps a trace of each.</summary>
public sealed class RegionalSpanReviewEngine(List<IRegionalSpanReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSpanReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSpanReviewEngine DefaultRegionalSpanReviewEngine()
    {
        return new RegionalSpanReviewEngine(
            new List<IRegionalSpanReviewFactor>
            {
                new PercentRegionalSpanReviewAdjuster(-15.7),
                new LabelRegionalSpanReviewAdjuster(new Dictionary<string, double> { ["north"] = 0.89 }),
                new RoundingRegionalSpanReviewAdjuster(0.1),
                new CapRegionalSpanReviewAdjuster(34506.0),
                new TieredRegionalSpanReviewAdjuster(
                    new List<double> { 678.0, 1812.0, 7479.0, 7512.0 },
                    new List<double> { 0.98, 1.03, 0.97, 0.86 }),
            },
            new List<string>(),
            3.0,
            36565.9);
    }

    public double Run(RegionalSpanReview item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSpanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
