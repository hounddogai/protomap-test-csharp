namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey claim calculation.</summary>
public interface IPulseSurveyClaimFactor
{
    string Describe();
    double Adjust(PulseSurveyClaim item, double valueValue);
}

public sealed record CapPulseSurveyClaimAdjuster(double Cap) : IPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPulseSurveyClaimAdjuster(double Percent) : IPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPulseSurveyClaimAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyClaim item, double valueValue)
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

/// <summary>Runs the pulse survey claim steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyClaimEngine(List<IPulseSurveyClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyClaimEngine DefaultPulseSurveyClaimEngine()
    {
        return new PulseSurveyClaimEngine(
            new List<IPulseSurveyClaimFactor>
            {
                new CapPulseSurveyClaimAdjuster(30294.0),
                new PercentPulseSurveyClaimAdjuster(-16.5),
                new TieredPulseSurveyClaimAdjuster(
                    new List<double> { 6129.0, 6220.0, 8835.0 },
                    new List<double> { 1.16, 0.92, 0.94 }),
            },
            new List<string>(),
            2.5,
            39998.8);
    }

    public double Price(PulseSurveyClaim item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
