namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey variance calculation.</summary>
public interface IStrictPulseSurveyVarianceStep
{
    string Describe();
    double Adjust(StrictPulseSurveyVariance item, double valueValue);
}

public sealed record RoundingStrictPulseSurveyVarianceAdjuster(double Step) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapStrictPulseSurveyVarianceAdjuster(double Cap) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStrictPulseSurveyVarianceAdjuster(double Percent) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictPulseSurveyVarianceAdjuster(double Minimum) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictPulseSurveyVarianceStartsAtOffsetAdjuster(double Weight) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record FlaggedStrictPulseSurveyVarianceAdjuster(Registry Flags, string Flag, double Percent) : IStrictPulseSurveyVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPulseSurveyVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict pulse survey variance steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyVarianceEngine(List<IStrictPulseSurveyVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyVarianceEngine DefaultStrictPulseSurveyVarianceEngine(Registry flags)
    {
        return new StrictPulseSurveyVarianceEngine(
            new List<IStrictPulseSurveyVarianceStep>
            {
                new RoundingStrictPulseSurveyVarianceAdjuster(1.0),
                new CapStrictPulseSurveyVarianceAdjuster(23543.0),
                new PercentStrictPulseSurveyVarianceAdjuster(-11.7),
                new MinimumStrictPulseSurveyVarianceAdjuster(13.0),
                new StrictPulseSurveyVarianceStartsAtOffsetAdjuster(1.01),
                new FlaggedStrictPulseSurveyVarianceAdjuster(flags, "strict-pulse-survey-variance-tiered", 3.6),
            },
            new List<string>(),
            1.3,
            78975.9);
    }

    public double Evaluate(StrictPulseSurveyVariance item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
