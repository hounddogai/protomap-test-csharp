namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped return plan review calculation.</summary>
public interface ICappedReturnPlanReviewFactor
{
    string Describe();
    double Adjust(CappedReturnPlanReview item, double valueValue);
}

public sealed record PercentCappedReturnPlanReviewAdjuster(double Percent) : ICappedReturnPlanReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedReturnPlanReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedReturnPlanReviewAdjuster(double Step) : ICappedReturnPlanReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedReturnPlanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedReturnPlanReviewAdjuster(List<double> Bounds, List<double> Rates) : ICappedReturnPlanReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedReturnPlanReview item, double valueValue)
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

public sealed record MinimumCappedReturnPlanReviewAdjuster(double Minimum) : ICappedReturnPlanReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedReturnPlanReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedReturnPlanReviewEndsAtOffsetAdjuster(double Weight) : ICappedReturnPlanReviewFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CappedReturnPlanReview item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the capped return plan review steps in order and keeps a trace of each.</summary>
public sealed class CappedReturnPlanReviewEngine(List<ICappedReturnPlanReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedReturnPlanReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedReturnPlanReviewEngine DefaultCappedReturnPlanReviewEngine()
    {
        return new CappedReturnPlanReviewEngine(
            new List<ICappedReturnPlanReviewFactor>
            {
                new PercentCappedReturnPlanReviewAdjuster(0.9),
                new RoundingCappedReturnPlanReviewAdjuster(0.5),
                new TieredCappedReturnPlanReviewAdjuster(
                    new List<double> { 721.0, 1612.0, 4780.0, 8070.0 },
                    new List<double> { 0.83, 0.99, 1.15, 1.13 }),
                new MinimumCappedReturnPlanReviewAdjuster(38.2),
                new CappedReturnPlanReviewEndsAtOffsetAdjuster(0.41),
            },
            new List<string>(),
            2.0,
            73095.6);
    }

    public double Compute(CappedReturnPlanReview item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedReturnPlanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
