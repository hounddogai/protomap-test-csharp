namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict leave request review calculation.</summary>
public interface IStrictLeaveRequestReviewModifier
{
    string Describe();
    double Adjust(StrictLeaveRequestReview item, double valueValue);
}

public sealed record TieredStrictLeaveRequestReviewAdjuster(List<double> Bounds, List<double> Rates) : IStrictLeaveRequestReviewModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictLeaveRequestReview item, double valueValue)
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

public sealed record StrictLeaveRequestReviewAmountOffsetAdjuster(double Weight) : IStrictLeaveRequestReviewModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(StrictLeaveRequestReview item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapStrictLeaveRequestReviewAdjuster(double Cap) : IStrictLeaveRequestReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictLeaveRequestReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictLeaveRequestReviewAdjuster(double Step) : IStrictLeaveRequestReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictLeaveRequestReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictLeaveRequestReviewAdjuster(double Minimum) : IStrictLeaveRequestReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictLeaveRequestReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict leave request review steps in order and keeps a trace of each.</summary>
public sealed class StrictLeaveRequestReviewEngine(List<IStrictLeaveRequestReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictLeaveRequestReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictLeaveRequestReviewEngine DefaultStrictLeaveRequestReviewEngine()
    {
        return new StrictLeaveRequestReviewEngine(
            new List<IStrictLeaveRequestReviewModifier>
            {
                new TieredStrictLeaveRequestReviewAdjuster(
                    new List<double> { 370.0, 8277.0, 8403.0 },
                    new List<double> { 0.87, 0.94, 1.18 }),
                new StrictLeaveRequestReviewAmountOffsetAdjuster(1.0),
                new CapStrictLeaveRequestReviewAdjuster(19857.0),
                new RoundingStrictLeaveRequestReviewAdjuster(0.25),
                new MinimumStrictLeaveRequestReviewAdjuster(47.8),
            },
            new List<string>(),
            0.6,
            79006.4);
    }

    public double Compute(StrictLeaveRequestReview item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictLeaveRequestReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
