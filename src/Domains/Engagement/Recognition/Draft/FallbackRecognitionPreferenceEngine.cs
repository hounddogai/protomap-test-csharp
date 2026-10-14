namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback recognition preference calculation.</summary>
public interface IFallbackRecognitionPreferenceAdjuster
{
    string Describe();
    double Adjust(FallbackRecognitionPreference item, double valueValue);
}

public sealed record PercentFallbackRecognitionPreferenceAdjuster(double Percent) : IFallbackRecognitionPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRecognitionPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackRecognitionPreferenceAdjuster(double Minimum) : IFallbackRecognitionPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRecognitionPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFallbackRecognitionPreferenceAdjuster(double Step) : IFallbackRecognitionPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackRecognitionPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFallbackRecognitionPreferenceAdjuster(double Cap) : IFallbackRecognitionPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackRecognitionPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback recognition preference steps in order and keeps a trace of each.</summary>
public sealed class FallbackRecognitionPreferenceEngine(List<IFallbackRecognitionPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRecognitionPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRecognitionPreferenceEngine DefaultFallbackRecognitionPreferenceEngine()
    {
        return new FallbackRecognitionPreferenceEngine(
            new List<IFallbackRecognitionPreferenceAdjuster>
            {
                new PercentFallbackRecognitionPreferenceAdjuster(21.9),
                new MinimumFallbackRecognitionPreferenceAdjuster(29.4),
                new RoundingFallbackRecognitionPreferenceAdjuster(0.1),
                new CapFallbackRecognitionPreferenceAdjuster(18788.0),
            },
            new List<string>(),
            4.0,
            70249.6);
    }

    public double Compute(FallbackRecognitionPreference item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRecognitionPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
