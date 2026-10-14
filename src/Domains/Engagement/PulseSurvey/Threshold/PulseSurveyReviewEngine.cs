namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey review calculation.</summary>
public interface IPulseSurveyReviewAdjuster
{
    string Describe();
    double Adjust(PulseSurveyReview item, double valueValue);
}

public sealed record FlaggedPulseSurveyReviewAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPulseSurveyReviewAdjuster(double Minimum) : IPulseSurveyReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPulseSurveyReviewAdjuster(double Step) : IPulseSurveyReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPulseSurveyReviewAdjuster(double Percent) : IPulseSurveyReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyReviewAdjuster(double Cap) : IPulseSurveyReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pulse survey review steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyReviewEngine(List<IPulseSurveyReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyReviewEngine DefaultPulseSurveyReviewEngine(Registry flags)
    {
        return new PulseSurveyReviewEngine(
            new List<IPulseSurveyReviewAdjuster>
            {
                new FlaggedPulseSurveyReviewAdjuster(flags, "pulse-survey-review-fallback", 8.3),
                new MinimumPulseSurveyReviewAdjuster(14.0),
                new RoundingPulseSurveyReviewAdjuster(5.0),
                new PercentPulseSurveyReviewAdjuster(-19.1),
                new CapPulseSurveyReviewAdjuster(44741.0),
            },
            new List<string>(),
            1.8,
            43199.5);
    }

    public double Evaluate(PulseSurveyReview item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
