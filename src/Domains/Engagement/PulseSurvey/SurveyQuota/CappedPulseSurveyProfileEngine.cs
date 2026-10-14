namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pulse survey profile calculation.</summary>
public interface ICappedPulseSurveyProfileAdjuster
{
    string Describe();
    double Adjust(CappedPulseSurveyProfile item, double valueValue);
}

public sealed record CappedPulseSurveyProfileWeightOffsetAdjuster(double Weight) : ICappedPulseSurveyProfileAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(CappedPulseSurveyProfile item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record TieredCappedPulseSurveyProfileAdjuster(List<double> Bounds, List<double> Rates) : ICappedPulseSurveyProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPulseSurveyProfile item, double valueValue)
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

public sealed record TitleCappedPulseSurveyProfileAdjuster(Dictionary<string, double> Factors) : ICappedPulseSurveyProfileAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(CappedPulseSurveyProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedPulseSurveyProfileAdjuster(double Step) : ICappedPulseSurveyProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPulseSurveyProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped pulse survey profile steps in order and keeps a trace of each.</summary>
public sealed class CappedPulseSurveyProfileEngine(List<ICappedPulseSurveyProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPulseSurveyProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPulseSurveyProfileEngine DefaultCappedPulseSurveyProfileEngine()
    {
        return new CappedPulseSurveyProfileEngine(
            new List<ICappedPulseSurveyProfileAdjuster>
            {
                new CappedPulseSurveyProfileWeightOffsetAdjuster(1.35),
                new TieredCappedPulseSurveyProfileAdjuster(
                    new List<double> { 4578.0, 9433.0 },
                    new List<double> { 1.01, 1.04 }),
                new TitleCappedPulseSurveyProfileAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.83, ["central"] = 1.26 }),
                new RoundingCappedPulseSurveyProfileAdjuster(1.0),
            },
            new List<string>(),
            1.5,
            88182.9);
    }

    public double Run(CappedPulseSurveyProfile item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPulseSurveyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
