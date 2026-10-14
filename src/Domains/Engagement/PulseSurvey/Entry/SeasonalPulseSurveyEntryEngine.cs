namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>One step of the seasonal pulse survey entry calculation.</summary>
public interface ISeasonalPulseSurveyEntryAdjuster
{
    string Describe();
    double Adjust(SeasonalPulseSurveyEntry item, double valueValue);
}

public sealed record PercentSeasonalPulseSurveyEntryAdjuster(double Percent) : ISeasonalPulseSurveyEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPulseSurveyEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategorySeasonalPulseSurveyEntryAdjuster(Dictionary<string, double> Factors) : ISeasonalPulseSurveyEntryAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SeasonalPulseSurveyEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSeasonalPulseSurveyEntryAdjuster(double Minimum) : ISeasonalPulseSurveyEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPulseSurveyEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal pulse survey entry steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPulseSurveyEntryEngine(List<ISeasonalPulseSurveyEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPulseSurveyEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPulseSurveyEntryEngine DefaultSeasonalPulseSurveyEntryEngine()
    {
        return new SeasonalPulseSurveyEntryEngine(
            new List<ISeasonalPulseSurveyEntryAdjuster>
            {
                new PercentSeasonalPulseSurveyEntryAdjuster(7.5),
                new CategorySeasonalPulseSurveyEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["central"] = 0.74 }),
                new MinimumSeasonalPulseSurveyEntryAdjuster(7.3),
            },
            new List<string>(),
            1.7,
            73126.7);
    }

    public double Run(SeasonalPulseSurveyEntry item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPulseSurveyEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
