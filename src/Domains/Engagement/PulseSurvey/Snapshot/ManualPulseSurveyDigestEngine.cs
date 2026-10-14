namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pulse survey digest calculation.</summary>
public interface IManualPulseSurveyDigestFactor
{
    string Describe();
    double Adjust(ManualPulseSurveyDigest item, double valueValue);
}

public sealed record RoundingManualPulseSurveyDigestAdjuster(double Step) : IManualPulseSurveyDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPulseSurveyDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameManualPulseSurveyDigestAdjuster(Dictionary<string, double> Factors) : IManualPulseSurveyDigestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualPulseSurveyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumManualPulseSurveyDigestAdjuster(double Minimum) : IManualPulseSurveyDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPulseSurveyDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual pulse survey digest steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveyDigestEngine(List<IManualPulseSurveyDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveyDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveyDigestEngine DefaultManualPulseSurveyDigestEngine()
    {
        return new ManualPulseSurveyDigestEngine(
            new List<IManualPulseSurveyDigestFactor>
            {
                new RoundingManualPulseSurveyDigestAdjuster(5.0),
                new NameManualPulseSurveyDigestAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.17, ["north"] = 0.9 }),
                new MinimumManualPulseSurveyDigestAdjuster(19.3),
            },
            new List<string>(),
            1.8,
            36833.0);
    }

    public double Evaluate(ManualPulseSurveyDigest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
