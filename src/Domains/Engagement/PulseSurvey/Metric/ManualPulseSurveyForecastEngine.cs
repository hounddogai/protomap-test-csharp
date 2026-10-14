namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pulse survey forecast calculation.</summary>
public interface IManualPulseSurveyForecastModifier
{
    string Describe();
    double Adjust(ManualPulseSurveyForecast item, double valueValue);
}

public sealed record FlaggedManualPulseSurveyForecastAdjuster(Registry Flags, string Flag, double Percent) : IManualPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPulseSurveyForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualPulseSurveyForecastAdjuster(double Step) : IManualPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPulseSurveyForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualPulseSurveyForecastAdjuster(double Cap) : IManualPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPulseSurveyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredManualPulseSurveyForecastAdjuster(List<double> Bounds, List<double> Rates) : IManualPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualPulseSurveyForecast item, double valueValue)
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

/// <summary>Runs the manual pulse survey forecast steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveyForecastEngine(List<IManualPulseSurveyForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveyForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveyForecastEngine DefaultManualPulseSurveyForecastEngine(Registry flags)
    {
        return new ManualPulseSurveyForecastEngine(
            new List<IManualPulseSurveyForecastModifier>
            {
                new FlaggedManualPulseSurveyForecastAdjuster(flags, "manual-pulse-survey-forecast-pending", -0.8),
                new RoundingManualPulseSurveyForecastAdjuster(5.0),
                new CapManualPulseSurveyForecastAdjuster(21432.0),
                new TieredManualPulseSurveyForecastAdjuster(
                    new List<double> { 936.0, 1778.0, 4047.0, 7838.0 },
                    new List<double> { 0.96, 1.03, 1.2, 1.01 }),
            },
            new List<string>(),
            3.1,
            91498.8);
    }

    public double Run(ManualPulseSurveyForecast item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
