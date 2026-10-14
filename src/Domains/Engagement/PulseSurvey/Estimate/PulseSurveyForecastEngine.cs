namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pulse survey forecast calculation.</summary>
public interface IPulseSurveyForecastModifier
{
    string Describe();
    double Adjust(PulseSurveyForecast item, double valueValue);
}

public sealed record TieredPulseSurveyForecastAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyForecast item, double valueValue)
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

public sealed record CapPulseSurveyForecastAdjuster(double Cap) : IPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPulseSurveyForecastAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PulseSurveyForecastFteOffsetAdjuster(double Weight) : IPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PulseSurveyForecast item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record PercentPulseSurveyForecastAdjuster(double Percent) : IPulseSurveyForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey forecast steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyForecastEngine(List<IPulseSurveyForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyForecastEngine DefaultPulseSurveyForecastEngine(Registry flags)
    {
        return new PulseSurveyForecastEngine(
            new List<IPulseSurveyForecastModifier>
            {
                new TieredPulseSurveyForecastAdjuster(
                    new List<double> { 1987.0, 2134.0, 3052.0, 3242.0 },
                    new List<double> { 1.11, 1.17, 1.03, 0.86 }),
                new CapPulseSurveyForecastAdjuster(41530.0),
                new FlaggedPulseSurveyForecastAdjuster(flags, "pulse-survey-forecast-global", 4.4),
                new PulseSurveyForecastFteOffsetAdjuster(1.44),
                new PercentPulseSurveyForecastAdjuster(-19.1),
            },
            new List<string>(),
            0.4,
            30474.1);
    }

    public double Price(PulseSurveyForecast item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
