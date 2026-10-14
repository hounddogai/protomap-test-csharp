namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey preference calculation.</summary>
public interface IPulseSurveyPreferenceAdjuster
{
    string Describe();
    double Adjust(PulseSurveyPreference item, double valueValue);
}

public sealed record PulseSurveyPreferenceScoreOffsetAdjuster(double Weight) : IPulseSurveyPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PulseSurveyPreference item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record SegmentPulseSurveyPreferenceAdjuster(Dictionary<string, double> Factors) : IPulseSurveyPreferenceAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PulseSurveyPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPulseSurveyPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPulseSurveyPreferenceAdjuster(double Step) : IPulseSurveyPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPulseSurveyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyPreference item, double valueValue)
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

/// <summary>Runs the pulse survey preference steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyPreferenceEngine(List<IPulseSurveyPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyPreferenceEngine DefaultPulseSurveyPreferenceEngine(Registry flags)
    {
        return new PulseSurveyPreferenceEngine(
            new List<IPulseSurveyPreferenceAdjuster>
            {
                new PulseSurveyPreferenceScoreOffsetAdjuster(0.46),
                new SegmentPulseSurveyPreferenceAdjuster(new Dictionary<string, double> { ["central"] = 1.08 }),
                new FlaggedPulseSurveyPreferenceAdjuster(flags, "pulse-survey-preference-legacy", 8.6),
                new RoundingPulseSurveyPreferenceAdjuster(0.1),
                new TieredPulseSurveyPreferenceAdjuster(
                    new List<double> { 1189.0, 4516.0, 9060.0, 9491.0 },
                    new List<double> { 1.14, 1.2, 0.93, 1.08 }),
            },
            new List<string>(),
            1.4,
            75452.7);
    }

    public double Compute(PulseSurveyPreference item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
