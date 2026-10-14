namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment review calculation.</summary>
public interface IAdjustmentReviewStep
{
    string Describe();
    double Adjust(AdjustmentReview item, double valueValue);
}

public sealed record PercentAdjustmentReviewAdjuster(double Percent) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentReviewAdjuster(double Minimum) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentReviewAdjuster(double Cap) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdjustmentReviewAdjuster(double Step) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdjustmentReviewAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryAdjustmentReviewAdjuster(Dictionary<string, double> Factors) : IAdjustmentReviewStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdjustmentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment review steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentReviewEngine(List<IAdjustmentReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentReviewEngine DefaultAdjustmentReviewEngine(Registry flags)
    {
        return new AdjustmentReviewEngine(
            new List<IAdjustmentReviewStep>
            {
                new PercentAdjustmentReviewAdjuster(20.1),
                new MinimumAdjustmentReviewAdjuster(4.3),
                new CapAdjustmentReviewAdjuster(47497.0),
                new RoundingAdjustmentReviewAdjuster(0.1),
                new FlaggedAdjustmentReviewAdjuster(flags, "adjustment-review-legacy", 5.3),
                new CountryAdjustmentReviewAdjuster(new Dictionary<string, double> { ["north"] = 1.0 }),
            },
            new List<string>(),
            2.3,
            65394.5);
    }

    public double Run(AdjustmentReview item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
