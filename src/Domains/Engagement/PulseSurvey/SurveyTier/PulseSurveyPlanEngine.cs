namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pulse survey plan calculation.</summary>
public interface IPulseSurveyPlanFactor
{
    string Describe();
    double Adjust(PulseSurveyPlan item, double valueValue);
}

public sealed record TieredPulseSurveyPlanAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyPlan item, double valueValue)
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

public sealed record MinimumPulseSurveyPlanAdjuster(double Minimum) : IPulseSurveyPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPulseSurveyPlanAdjuster(double Percent) : IPulseSurveyPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPulseSurveyPlanAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey plan steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyPlanEngine(List<IPulseSurveyPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyPlanEngine DefaultPulseSurveyPlanEngine(Registry flags)
    {
        return new PulseSurveyPlanEngine(
            new List<IPulseSurveyPlanFactor>
            {
                new TieredPulseSurveyPlanAdjuster(
                    new List<double> { 2668.0, 4705.0 },
                    new List<double> { 1.11, 1.01 }),
                new MinimumPulseSurveyPlanAdjuster(28.9),
                new PercentPulseSurveyPlanAdjuster(17.5),
                new FlaggedPulseSurveyPlanAdjuster(flags, "pulse-survey-plan-rolling", -6.1),
            },
            new List<string>(),
            0.1,
            90598.9);
    }

    public double Compute(PulseSurveyPlan item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
