namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>One step of the seasonal feedback score calculation.</summary>
public interface ISeasonalFeedbackScoreFactor
{
    string Describe();
    double Adjust(SeasonalFeedbackScore item, double valueValue);
}

public sealed record CountrySeasonalFeedbackScoreAdjuster(Dictionary<string, double> Factors) : ISeasonalFeedbackScoreFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(SeasonalFeedbackScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalFeedbackScoreAdjuster(double Cap) : ISeasonalFeedbackScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFeedbackScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSeasonalFeedbackScoreAdjuster(double Percent) : ISeasonalFeedbackScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalFeedbackScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal feedback score steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFeedbackScoreEngine(List<ISeasonalFeedbackScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFeedbackScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFeedbackScoreEngine DefaultSeasonalFeedbackScoreEngine()
    {
        return new SeasonalFeedbackScoreEngine(
            new List<ISeasonalFeedbackScoreFactor>
            {
                new CountrySeasonalFeedbackScoreAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.84, ["south"] = 1.07 }),
                new CapSeasonalFeedbackScoreAdjuster(39812.0),
                new PercentSeasonalFeedbackScoreAdjuster(-24.5),
            },
            new List<string>(),
            4.1,
            1798.7);
    }

    public double Evaluate(SeasonalFeedbackScore item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFeedbackScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
