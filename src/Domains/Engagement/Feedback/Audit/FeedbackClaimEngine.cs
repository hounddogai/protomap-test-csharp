namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback claim calculation.</summary>
public interface IFeedbackClaimStep
{
    string Describe();
    double Adjust(FeedbackClaim item, double valueValue);
}

public sealed record TieredFeedbackClaimAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackClaim item, double valueValue)
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

public sealed record RegionFeedbackClaimAdjuster(Dictionary<string, double> Factors) : IFeedbackClaimStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FeedbackClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFeedbackClaimAdjuster(double Cap) : IFeedbackClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the feedback claim steps in order and keeps a trace of each.</summary>
public sealed class FeedbackClaimEngine(List<IFeedbackClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackClaimEngine DefaultFeedbackClaimEngine()
    {
        return new FeedbackClaimEngine(
            new List<IFeedbackClaimStep>
            {
                new TieredFeedbackClaimAdjuster(
                    new List<double> { 1006.0, 3158.0, 4406.0, 6742.0 },
                    new List<double> { 0.85, 0.89, 1.0, 0.97 }),
                new RegionFeedbackClaimAdjuster(new Dictionary<string, double> { ["south"] = 0.74 }),
                new CapFeedbackClaimAdjuster(2878.0),
            },
            new List<string>(),
            2.2,
            65087.4);
    }

    public double Evaluate(FeedbackClaim item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
