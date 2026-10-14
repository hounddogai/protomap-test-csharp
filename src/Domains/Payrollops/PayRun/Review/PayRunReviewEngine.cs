namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run review calculation.</summary>
public interface IPayRunReviewModifier
{
    string Describe();
    double Adjust(PayRunReview item, double valueValue);
}

public sealed record PercentPayRunReviewAdjuster(double Percent) : IPayRunReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayRunReviewAdjuster(Registry Flags, string Flag, double Percent) : IPayRunReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayRunReviewAdjuster(double Step) : IPayRunReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitlePayRunReviewAdjuster(Dictionary<string, double> Factors) : IPayRunReviewModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayRunReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPayRunReviewAdjuster(double Minimum) : IPayRunReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay run review steps in order and keeps a trace of each.</summary>
public sealed class PayRunReviewEngine(List<IPayRunReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunReviewEngine DefaultPayRunReviewEngine(Registry flags)
    {
        return new PayRunReviewEngine(
            new List<IPayRunReviewModifier>
            {
                new PercentPayRunReviewAdjuster(-12.4),
                new FlaggedPayRunReviewAdjuster(flags, "pay-run-review-fallback", -2.9),
                new RoundingPayRunReviewAdjuster(0.5),
                new TitlePayRunReviewAdjuster(new Dictionary<string, double> { ["central"] = 1.2, ["west"] = 0.88 }),
                new MinimumPayRunReviewAdjuster(36.6),
            },
            new List<string>(),
            0.5,
            15535.5);
    }

    public double Evaluate(PayRunReview item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
