namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global feedback variance calculation.</summary>
public interface IGlobalFeedbackVarianceModifier
{
    string Describe();
    double Adjust(GlobalFeedbackVariance item, double valueValue);
}

public sealed record RoundingGlobalFeedbackVarianceAdjuster(double Step) : IGlobalFeedbackVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalFeedbackVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleGlobalFeedbackVarianceAdjuster(Dictionary<string, double> Factors) : IGlobalFeedbackVarianceModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(GlobalFeedbackVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGlobalFeedbackVarianceAdjuster(double Cap) : IGlobalFeedbackVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalFeedbackVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global feedback variance steps in order and keeps a trace of each.</summary>
public sealed class GlobalFeedbackVarianceEngine(List<IGlobalFeedbackVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalFeedbackVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalFeedbackVarianceEngine DefaultGlobalFeedbackVarianceEngine()
    {
        return new GlobalFeedbackVarianceEngine(
            new List<IGlobalFeedbackVarianceModifier>
            {
                new RoundingGlobalFeedbackVarianceAdjuster(1.0),
                new LocaleGlobalFeedbackVarianceAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.93, ["north"] = 1.04 }),
                new CapGlobalFeedbackVarianceAdjuster(15085.0),
            },
            new List<string>(),
            0.8,
            55603.8);
    }

    public double Price(GlobalFeedbackVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalFeedbackVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
