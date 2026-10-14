namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey plan calculation.</summary>
public interface IStrictPulseSurveyPlanAdjuster
{
    string Describe();
    double Adjust(StrictPulseSurveyPlan item, double valueValue);
}

public sealed record RoundingStrictPulseSurveyPlanAdjuster(double Step) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictPulseSurveyPlanAdjuster(double Percent) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictPulseSurveyPlanAdjuster(double Minimum) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictPulseSurveyPlanAdjuster(double Cap) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictPulseSurveyPlanAdjuster(List<double> Bounds, List<double> Rates) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
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

public sealed record StrictPulseSurveyPlanCreatedAtOffsetAdjuster(double Weight) : IStrictPulseSurveyPlanAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(StrictPulseSurveyPlan item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the strict pulse survey plan steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyPlanEngine(List<IStrictPulseSurveyPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyPlanEngine DefaultStrictPulseSurveyPlanEngine()
    {
        return new StrictPulseSurveyPlanEngine(
            new List<IStrictPulseSurveyPlanAdjuster>
            {
                new RoundingStrictPulseSurveyPlanAdjuster(0.1),
                new PercentStrictPulseSurveyPlanAdjuster(14.0),
                new MinimumStrictPulseSurveyPlanAdjuster(32.3),
                new CapStrictPulseSurveyPlanAdjuster(8878.0),
                new TieredStrictPulseSurveyPlanAdjuster(
                    new List<double> { 2490.0, 7183.0, 8011.0 },
                    new List<double> { 1.19, 0.84, 0.83 }),
                new StrictPulseSurveyPlanCreatedAtOffsetAdjuster(1.41),
            },
            new List<string>(),
            1.9,
            38916.8);
    }

    public double Compute(StrictPulseSurveyPlan item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
