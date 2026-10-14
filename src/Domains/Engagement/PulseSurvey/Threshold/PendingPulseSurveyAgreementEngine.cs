namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pulse survey agreement calculation.</summary>
public interface IPendingPulseSurveyAgreementFactor
{
    string Describe();
    double Adjust(PendingPulseSurveyAgreement item, double valueValue);
}

public sealed record PendingPulseSurveyAgreementCreatedAtOffsetAdjuster(double Weight) : IPendingPulseSurveyAgreementFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PendingPulseSurveyAgreement item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record TieredPendingPulseSurveyAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPendingPulseSurveyAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPulseSurveyAgreement item, double valueValue)
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

public sealed record MinimumPendingPulseSurveyAgreementAdjuster(double Minimum) : IPendingPulseSurveyAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPulseSurveyAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingPulseSurveyAgreementAdjuster(double Percent) : IPendingPulseSurveyAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPulseSurveyAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending pulse survey agreement steps in order and keeps a trace of each.</summary>
public sealed class PendingPulseSurveyAgreementEngine(List<IPendingPulseSurveyAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPulseSurveyAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPulseSurveyAgreementEngine DefaultPendingPulseSurveyAgreementEngine()
    {
        return new PendingPulseSurveyAgreementEngine(
            new List<IPendingPulseSurveyAgreementFactor>
            {
                new PendingPulseSurveyAgreementCreatedAtOffsetAdjuster(1.5),
                new TieredPendingPulseSurveyAgreementAdjuster(
                    new List<double> { 2245.0, 5506.0, 5873.0 },
                    new List<double> { 1.06, 1.16, 1.09 }),
                new MinimumPendingPulseSurveyAgreementAdjuster(15.7),
                new PercentPendingPulseSurveyAgreementAdjuster(-7.0),
            },
            new List<string>(),
            0.2,
            56735.0);
    }

    public double Compute(PendingPulseSurveyAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingPulseSurveyAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
