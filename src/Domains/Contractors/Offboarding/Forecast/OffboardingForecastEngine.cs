namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding forecast calculation.</summary>
public interface IOffboardingForecastFactor
{
    string Describe();
    double Adjust(OffboardingForecast item, double valueValue);
}

public sealed record PercentOffboardingForecastAdjuster(double Percent) : IOffboardingForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOffboardingForecastAdjuster(double Step) : IOffboardingForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedOffboardingForecastAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding forecast steps in order and keeps a trace of each.</summary>
public sealed class OffboardingForecastEngine(List<IOffboardingForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingForecastEngine DefaultOffboardingForecastEngine(Registry flags)
    {
        return new OffboardingForecastEngine(
            new List<IOffboardingForecastFactor>
            {
                new PercentOffboardingForecastAdjuster(-13.0),
                new RoundingOffboardingForecastAdjuster(0.25),
                new FlaggedOffboardingForecastAdjuster(flags, "offboarding-forecast-manual", -4.8),
            },
            new List<string>(),
            0.4,
            69125.2);
    }

    public double Run(OffboardingForecast item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
