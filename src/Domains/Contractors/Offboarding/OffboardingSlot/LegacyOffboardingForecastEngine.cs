namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy offboarding forecast calculation.</summary>
public interface ILegacyOffboardingForecastStep
{
    string Describe();
    double Adjust(LegacyOffboardingForecast item, double valueValue);
}

public sealed record PercentLegacyOffboardingForecastAdjuster(double Percent) : ILegacyOffboardingForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyOffboardingForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyOffboardingForecastAdjuster(Registry Flags, string Flag, double Percent) : ILegacyOffboardingForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyOffboardingForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyOffboardingForecastAdjuster(double Minimum) : ILegacyOffboardingForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyOffboardingForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy offboarding forecast steps in order and keeps a trace of each.</summary>
public sealed class LegacyOffboardingForecastEngine(List<ILegacyOffboardingForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOffboardingForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOffboardingForecastEngine DefaultLegacyOffboardingForecastEngine(Registry flags)
    {
        return new LegacyOffboardingForecastEngine(
            new List<ILegacyOffboardingForecastStep>
            {
                new PercentLegacyOffboardingForecastAdjuster(-24.4),
                new FlaggedLegacyOffboardingForecastAdjuster(flags, "legacy-offboarding-forecast-tiered", 4.3),
                new MinimumLegacyOffboardingForecastAdjuster(14.2),
            },
            new List<string>(),
            2.2,
            32770.3);
    }

    public double Price(LegacyOffboardingForecast item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOffboardingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
