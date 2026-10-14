namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pulse survey claim calculation.</summary>
public interface IPendingPulseSurveyClaimStep
{
    string Describe();
    double Adjust(PendingPulseSurveyClaim item, double valueValue);
}

public sealed record TieredPendingPulseSurveyClaimAdjuster(List<double> Bounds, List<double> Rates) : IPendingPulseSurveyClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPulseSurveyClaim item, double valueValue)
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

public sealed record PendingPulseSurveyClaimMinimumOffsetAdjuster(double Weight) : IPendingPulseSurveyClaimStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PendingPulseSurveyClaim item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingPendingPulseSurveyClaimAdjuster(double Step) : IPendingPulseSurveyClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingPulseSurveyClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPendingPulseSurveyClaimAdjuster(double Percent) : IPendingPulseSurveyClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPulseSurveyClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingPulseSurveyClaimAdjuster(Registry Flags, string Flag, double Percent) : IPendingPulseSurveyClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingPulseSurveyClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending pulse survey claim steps in order and keeps a trace of each.</summary>
public sealed class PendingPulseSurveyClaimEngine(List<IPendingPulseSurveyClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPulseSurveyClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPulseSurveyClaimEngine DefaultPendingPulseSurveyClaimEngine(Registry flags)
    {
        return new PendingPulseSurveyClaimEngine(
            new List<IPendingPulseSurveyClaimStep>
            {
                new TieredPendingPulseSurveyClaimAdjuster(
                    new List<double> { 3196.0, 4873.0, 6607.0, 9133.0 },
                    new List<double> { 1.18, 1.2, 0.8, 1.16 }),
                new PendingPulseSurveyClaimMinimumOffsetAdjuster(1.15),
                new RoundingPendingPulseSurveyClaimAdjuster(0.5),
                new PercentPendingPulseSurveyClaimAdjuster(9.9),
                new FlaggedPendingPulseSurveyClaimAdjuster(flags, "pending-pulse-survey-claim-manual", -1.9),
            },
            new List<string>(),
            3.7,
            67795.8);
    }

    public double Evaluate(PendingPulseSurveyClaim item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPulseSurveyClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
