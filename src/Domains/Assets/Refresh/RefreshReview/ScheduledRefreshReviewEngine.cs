namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled refresh review calculation.</summary>
public interface IScheduledRefreshReviewStep
{
    string Describe();
    double Adjust(ScheduledRefreshReview item, double valueValue);
}

public sealed record CapScheduledRefreshReviewAdjuster(double Cap) : IScheduledRefreshReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRefreshReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleScheduledRefreshReviewAdjuster(Dictionary<string, double> Factors) : IScheduledRefreshReviewStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledRefreshReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledRefreshReviewAdjuster(List<double> Bounds, List<double> Rates) : IScheduledRefreshReviewStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledRefreshReview item, double valueValue)
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

public sealed record RoundingScheduledRefreshReviewAdjuster(double Step) : IScheduledRefreshReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledRefreshReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled refresh review steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRefreshReviewEngine(List<IScheduledRefreshReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRefreshReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRefreshReviewEngine DefaultScheduledRefreshReviewEngine()
    {
        return new ScheduledRefreshReviewEngine(
            new List<IScheduledRefreshReviewStep>
            {
                new CapScheduledRefreshReviewAdjuster(34147.0),
                new TitleScheduledRefreshReviewAdjuster(new Dictionary<string, double> { ["south"] = 0.81 }),
                new TieredScheduledRefreshReviewAdjuster(
                    new List<double> { 2728.0, 5493.0 },
                    new List<double> { 0.87, 1.02 }),
                new RoundingScheduledRefreshReviewAdjuster(1.0),
            },
            new List<string>(),
            4.4,
            30635.4);
    }

    public double Run(ScheduledRefreshReview item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRefreshReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
