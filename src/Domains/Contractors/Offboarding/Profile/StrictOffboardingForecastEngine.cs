namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict offboarding forecast calculation.</summary>
public interface IStrictOffboardingForecastModifier
{
    string Describe();
    double Adjust(StrictOffboardingForecast item, double valueValue);
}

public sealed record FlaggedStrictOffboardingForecastAdjuster(Registry Flags, string Flag, double Percent) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictOffboardingForecastAdjuster(double Step) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictOffboardingForecastMarginOffsetAdjuster(double Weight) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TitleStrictOffboardingForecastAdjuster(Dictionary<string, double> Factors) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictOffboardingForecastAdjuster(double Minimum) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictOffboardingForecastAdjuster(List<double> Bounds, List<double> Rates) : IStrictOffboardingForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictOffboardingForecast item, double valueValue)
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

/// <summary>Runs the strict offboarding forecast steps in order and keeps a trace of each.</summary>
public sealed class StrictOffboardingForecastEngine(List<IStrictOffboardingForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictOffboardingForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictOffboardingForecastEngine DefaultStrictOffboardingForecastEngine(Registry flags)
    {
        return new StrictOffboardingForecastEngine(
            new List<IStrictOffboardingForecastModifier>
            {
                new FlaggedStrictOffboardingForecastAdjuster(flags, "strict-offboarding-forecast-batched", 5.0),
                new RoundingStrictOffboardingForecastAdjuster(5.0),
                new StrictOffboardingForecastMarginOffsetAdjuster(1.2),
                new TitleStrictOffboardingForecastAdjuster(new Dictionary<string, double> { ["central"] = 1.08 }),
                new MinimumStrictOffboardingForecastAdjuster(19.6),
                new TieredStrictOffboardingForecastAdjuster(
                    new List<double> { 364.0, 586.0 },
                    new List<double> { 1.09, 1.18 }),
            },
            new List<string>(),
            4.1,
            26187.5);
    }

    public double Evaluate(StrictOffboardingForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictOffboardingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
