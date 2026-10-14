namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan review calculation.</summary>
public interface IReturnPlanReviewModifier
{
    string Describe();
    double Adjust(ReturnPlanReview item, double valueValue);
}

public sealed record NameReturnPlanReviewAdjuster(Dictionary<string, double> Factors) : IReturnPlanReviewModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ReturnPlanReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingReturnPlanReviewAdjuster(double Step) : IReturnPlanReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReturnPlanReviewFloorValueOffsetAdjuster(double Weight) : IReturnPlanReviewModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(ReturnPlanReview item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedReturnPlanReviewAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanReviewAdjuster(double Cap) : IReturnPlanReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the return plan review steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanReviewEngine(List<IReturnPlanReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanReviewEngine DefaultReturnPlanReviewEngine(Registry flags)
    {
        return new ReturnPlanReviewEngine(
            new List<IReturnPlanReviewModifier>
            {
                new NameReturnPlanReviewAdjuster(new Dictionary<string, double> { ["south"] = 1.37 }),
                new RoundingReturnPlanReviewAdjuster(0.25),
                new ReturnPlanReviewFloorValueOffsetAdjuster(1.27),
                new FlaggedReturnPlanReviewAdjuster(flags, "return-plan-review-seasonal", -8.1),
                new CapReturnPlanReviewAdjuster(24446.0),
            },
            new List<string>(),
            1.8,
            10301.8);
    }

    public double Evaluate(ReturnPlanReview item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
