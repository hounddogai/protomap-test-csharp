namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient pulse survey claim calculation.</summary>
public interface ILenientPulseSurveyClaimFactor
{
    string Describe();
    double Adjust(LenientPulseSurveyClaim item, double valueValue);
}

public sealed record CapLenientPulseSurveyClaimAdjuster(double Cap) : ILenientPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPulseSurveyClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientPulseSurveyClaimAdjuster(double Percent) : ILenientPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPulseSurveyClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPulseSurveyClaimAdjuster(double Minimum) : ILenientPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPulseSurveyClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleLenientPulseSurveyClaimAdjuster(Dictionary<string, double> Factors) : ILenientPulseSurveyClaimFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientPulseSurveyClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient pulse survey claim steps in order and keeps a trace of each.</summary>
public sealed class LenientPulseSurveyClaimEngine(List<ILenientPulseSurveyClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPulseSurveyClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPulseSurveyClaimEngine DefaultLenientPulseSurveyClaimEngine()
    {
        return new LenientPulseSurveyClaimEngine(
            new List<ILenientPulseSurveyClaimFactor>
            {
                new CapLenientPulseSurveyClaimAdjuster(46047.0),
                new PercentLenientPulseSurveyClaimAdjuster(-21.9),
                new MinimumLenientPulseSurveyClaimAdjuster(34.2),
                new TitleLenientPulseSurveyClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.36, ["south"] = 1.31, ["west"] = 0.8 }),
            },
            new List<string>(),
            0.3,
            4811.2);
    }

    public double Evaluate(LenientPulseSurveyClaim item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPulseSurveyClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
