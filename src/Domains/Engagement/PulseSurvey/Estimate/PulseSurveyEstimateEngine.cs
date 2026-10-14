namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey estimate calculation.</summary>
public interface IPulseSurveyEstimateStep
{
    string Describe();
    double Adjust(PulseSurveyEstimate item, double valueValue);
}

public sealed record PercentPulseSurveyEstimateAdjuster(double Percent) : IPulseSurveyEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPulseSurveyEstimateAdjuster(double Step) : IPulseSurveyEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPulseSurveyEstimateAdjuster(double Cap) : IPulseSurveyEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pulse survey estimate steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyEstimateEngine(List<IPulseSurveyEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyEstimateEngine DefaultPulseSurveyEstimateEngine()
    {
        return new PulseSurveyEstimateEngine(
            new List<IPulseSurveyEstimateStep>
            {
                new PercentPulseSurveyEstimateAdjuster(13.6),
                new RoundingPulseSurveyEstimateAdjuster(1.0),
                new CapPulseSurveyEstimateAdjuster(3351.0),
            },
            new List<string>(),
            3.6,
            78898.7);
    }

    public double Evaluate(PulseSurveyEstimate item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
