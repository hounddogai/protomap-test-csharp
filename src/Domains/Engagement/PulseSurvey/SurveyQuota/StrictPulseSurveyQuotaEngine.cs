namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey quota calculation.</summary>
public interface IStrictPulseSurveyQuotaAdjuster
{
    string Describe();
    double Adjust(StrictPulseSurveyQuota item, double valueValue);
}

public sealed record PercentStrictPulseSurveyQuotaAdjuster(double Percent) : IStrictPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPulseSurveyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPulseSurveyQuotaAdjuster(List<double> Bounds, List<double> Rates) : IStrictPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPulseSurveyQuota item, double valueValue)
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

public sealed record MinimumStrictPulseSurveyQuotaAdjuster(double Minimum) : IStrictPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPulseSurveyQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictPulseSurveyQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStrictPulseSurveyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPulseSurveyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict pulse survey quota steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyQuotaEngine(List<IStrictPulseSurveyQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyQuotaEngine DefaultStrictPulseSurveyQuotaEngine(Registry flags)
    {
        return new StrictPulseSurveyQuotaEngine(
            new List<IStrictPulseSurveyQuotaAdjuster>
            {
                new PercentStrictPulseSurveyQuotaAdjuster(3.7),
                new TieredStrictPulseSurveyQuotaAdjuster(
                    new List<double> { 497.0, 2672.0, 3193.0, 3911.0 },
                    new List<double> { 0.95, 1.09, 0.93, 1.08 }),
                new MinimumStrictPulseSurveyQuotaAdjuster(2.9),
                new FlaggedStrictPulseSurveyQuotaAdjuster(flags, "strict-pulse-survey-quota-batched", -1.3),
            },
            new List<string>(),
            3.9,
            13837.9);
    }

    public double Compute(StrictPulseSurveyQuota item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
