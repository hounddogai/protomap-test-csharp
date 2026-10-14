namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>One step of the pulse survey request calculation.</summary>
public interface IPulseSurveyRequestAdjuster
{
    string Describe();
    double Adjust(PulseSurveyRequest item, double valueValue);
}

public sealed record PercentPulseSurveyRequestAdjuster(double Percent) : IPulseSurveyRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyRequestAdjuster(double Cap) : IPulseSurveyRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPulseSurveyRequestAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyRequest item, double valueValue)
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

/// <summary>Runs the pulse survey request steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyRequestEngine(List<IPulseSurveyRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyRequestEngine DefaultPulseSurveyRequestEngine()
    {
        return new PulseSurveyRequestEngine(
            new List<IPulseSurveyRequestAdjuster>
            {
                new PercentPulseSurveyRequestAdjuster(19.4),
                new CapPulseSurveyRequestAdjuster(34889.0),
                new TieredPulseSurveyRequestAdjuster(
                    new List<double> { 1267.0, 4843.0, 6461.0, 6955.0 },
                    new List<double> { 1.11, 0.96, 1.09, 0.82 }),
            },
            new List<string>(),
            2.0,
            33968.4);
    }

    public double Price(PulseSurveyRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
