namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey draft calculation.</summary>
public interface IStrictPulseSurveyDraftStep
{
    string Describe();
    double Adjust(StrictPulseSurveyDraft item, double valueValue);
}

public sealed record RoundingStrictPulseSurveyDraftAdjuster(double Step) : IStrictPulseSurveyDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPulseSurveyDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictPulseSurveyDraftAdjuster(double Percent) : IStrictPulseSurveyDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPulseSurveyDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictPulseSurveyDraftRevisionOffsetAdjuster(double Weight) : IStrictPulseSurveyDraftStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(StrictPulseSurveyDraft item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record MinimumStrictPulseSurveyDraftAdjuster(double Minimum) : IStrictPulseSurveyDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPulseSurveyDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictPulseSurveyDraftAdjuster(Registry Flags, string Flag, double Percent) : IStrictPulseSurveyDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPulseSurveyDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict pulse survey draft steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyDraftEngine(List<IStrictPulseSurveyDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyDraftEngine DefaultStrictPulseSurveyDraftEngine(Registry flags)
    {
        return new StrictPulseSurveyDraftEngine(
            new List<IStrictPulseSurveyDraftStep>
            {
                new RoundingStrictPulseSurveyDraftAdjuster(0.25),
                new PercentStrictPulseSurveyDraftAdjuster(24.7),
                new StrictPulseSurveyDraftRevisionOffsetAdjuster(1.73),
                new MinimumStrictPulseSurveyDraftAdjuster(22.1),
                new FlaggedStrictPulseSurveyDraftAdjuster(flags, "strict-pulse-survey-draft-fallback", 7.0),
            },
            new List<string>(),
            2.9,
            71581.9);
    }

    public double Price(StrictPulseSurveyDraft item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
