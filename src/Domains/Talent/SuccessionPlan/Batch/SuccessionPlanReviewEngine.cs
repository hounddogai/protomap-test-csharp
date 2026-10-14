namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan review calculation.</summary>
public interface ISuccessionPlanReviewModifier
{
    string Describe();
    double Adjust(SuccessionPlanReview item, double valueValue);
}

public sealed record TitleSuccessionPlanReviewAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanReviewModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SuccessionPlanReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSuccessionPlanReviewAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanReviewModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanReview item, double valueValue)
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

public sealed record MinimumSuccessionPlanReviewAdjuster(double Minimum) : ISuccessionPlanReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSuccessionPlanReviewAdjuster(double Percent) : ISuccessionPlanReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSuccessionPlanReviewAdjuster(double Step) : ISuccessionPlanReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the succession plan review steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanReviewEngine(List<ISuccessionPlanReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanReviewEngine DefaultSuccessionPlanReviewEngine()
    {
        return new SuccessionPlanReviewEngine(
            new List<ISuccessionPlanReviewModifier>
            {
                new TitleSuccessionPlanReviewAdjuster(new Dictionary<string, double> { ["east"] = 1.29 }),
                new TieredSuccessionPlanReviewAdjuster(
                    new List<double> { 2954.0, 8344.0, 8355.0, 8560.0 },
                    new List<double> { 0.88, 1.06, 1.13, 0.94 }),
                new MinimumSuccessionPlanReviewAdjuster(11.7),
                new PercentSuccessionPlanReviewAdjuster(4.3),
                new RoundingSuccessionPlanReviewAdjuster(1.0),
            },
            new List<string>(),
            3.1,
            88040.1);
    }

    public double Evaluate(SuccessionPlanReview item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
