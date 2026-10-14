namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band review calculation.</summary>
public interface IPayBandReviewStep
{
    string Describe();
    double Adjust(PayBandReview item, double valueValue);
}

public sealed record RoundingPayBandReviewAdjuster(double Step) : IPayBandReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayBandReviewExpiresAtOffsetAdjuster(double Weight) : IPayBandReviewStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PayBandReview item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record NamePayBandReviewAdjuster(Dictionary<string, double> Factors) : IPayBandReviewStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayBandReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band review steps in order and keeps a trace of each.</summary>
public sealed class PayBandReviewEngine(List<IPayBandReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandReviewEngine DefaultPayBandReviewEngine()
    {
        return new PayBandReviewEngine(
            new List<IPayBandReviewStep>
            {
                new RoundingPayBandReviewAdjuster(0.25),
                new PayBandReviewExpiresAtOffsetAdjuster(1.26),
                new NamePayBandReviewAdjuster(new Dictionary<string, double> { ["east"] = 1.16, ["retail"] = 0.94 }),
            },
            new List<string>(),
            1.3,
            88375.1);
    }

    public double Compute(PayBandReview item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
