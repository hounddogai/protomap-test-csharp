namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey preference calculation.</summary>
public interface IStrictPulseSurveyPreferenceFactor
{
    string Describe();
    double Adjust(StrictPulseSurveyPreference item, double valueValue);
}

public sealed record CapStrictPulseSurveyPreferenceAdjuster(double Cap) : IStrictPulseSurveyPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPulseSurveyPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictPulseSurveyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IStrictPulseSurveyPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPulseSurveyPreference item, double valueValue)
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

public sealed record StrictPulseSurveyPreferenceWindowDaysOffsetAdjuster(double Weight) : IStrictPulseSurveyPreferenceFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(StrictPulseSurveyPreference item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the strict pulse survey preference steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyPreferenceEngine(List<IStrictPulseSurveyPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyPreferenceEngine DefaultStrictPulseSurveyPreferenceEngine()
    {
        return new StrictPulseSurveyPreferenceEngine(
            new List<IStrictPulseSurveyPreferenceFactor>
            {
                new CapStrictPulseSurveyPreferenceAdjuster(13388.0),
                new TieredStrictPulseSurveyPreferenceAdjuster(
                    new List<double> { 4813.0, 9544.0 },
                    new List<double> { 1.16, 0.81 }),
                new StrictPulseSurveyPreferenceWindowDaysOffsetAdjuster(0.86),
            },
            new List<string>(),
            2.4,
            57204.5);
    }

    public double Price(StrictPulseSurveyPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
