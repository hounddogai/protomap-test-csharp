namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span review calculation.</summary>
public interface ISpanReviewAdjuster
{
    string Describe();
    double Adjust(SpanReview item, double valueValue);
}

public sealed record RegionSpanReviewAdjuster(Dictionary<string, double> Factors) : ISpanReviewAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SpanReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SpanReviewGraceDaysOffsetAdjuster(double Weight) : ISpanReviewAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SpanReview item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record RoundingSpanReviewAdjuster(double Step) : ISpanReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the span review steps in order and keeps a trace of each.</summary>
public sealed class SpanReviewEngine(List<ISpanReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanReviewEngine DefaultSpanReviewEngine()
    {
        return new SpanReviewEngine(
            new List<ISpanReviewAdjuster>
            {
                new RegionSpanReviewAdjuster(new Dictionary<string, double> { ["south"] = 1.31 }),
                new SpanReviewGraceDaysOffsetAdjuster(0.52),
                new RoundingSpanReviewAdjuster(1.0),
            },
            new List<string>(),
            1.6,
            35125.5);
    }

    public double Price(SpanReview item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SpanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
