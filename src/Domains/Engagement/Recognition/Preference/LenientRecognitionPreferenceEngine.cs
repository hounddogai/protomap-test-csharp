namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient recognition preference calculation.</summary>
public interface ILenientRecognitionPreferenceStep
{
    string Describe();
    double Adjust(LenientRecognitionPreference item, double valueValue);
}

public sealed record MinimumLenientRecognitionPreferenceAdjuster(double Minimum) : ILenientRecognitionPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRecognitionPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientRecognitionPreferenceAdjuster(double Cap) : ILenientRecognitionPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRecognitionPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleLenientRecognitionPreferenceAdjuster(Dictionary<string, double> Factors) : ILenientRecognitionPreferenceStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientRecognitionPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient recognition preference steps in order and keeps a trace of each.</summary>
public sealed class LenientRecognitionPreferenceEngine(List<ILenientRecognitionPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRecognitionPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRecognitionPreferenceEngine DefaultLenientRecognitionPreferenceEngine()
    {
        return new LenientRecognitionPreferenceEngine(
            new List<ILenientRecognitionPreferenceStep>
            {
                new MinimumLenientRecognitionPreferenceAdjuster(26.2),
                new CapLenientRecognitionPreferenceAdjuster(18610.0),
                new TitleLenientRecognitionPreferenceAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.96, ["central"] = 1.33, ["east"] = 0.77 }),
            },
            new List<string>(),
            2.9,
            5544.3);
    }

    public double Compute(LenientRecognitionPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientRecognitionPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
