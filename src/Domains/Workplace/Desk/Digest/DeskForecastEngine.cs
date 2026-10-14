namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk forecast calculation.</summary>
public interface IDeskForecastModifier
{
    string Describe();
    double Adjust(DeskForecast item, double valueValue);
}

public sealed record RoundingDeskForecastAdjuster(double Step) : IDeskForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDeskForecastAdjuster(Registry Flags, string Flag, double Percent) : IDeskForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeskForecastAdjuster(double Percent) : IDeskForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk forecast steps in order and keeps a trace of each.</summary>
public sealed class DeskForecastEngine(List<IDeskForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskForecastEngine DefaultDeskForecastEngine(Registry flags)
    {
        return new DeskForecastEngine(
            new List<IDeskForecastModifier>
            {
                new RoundingDeskForecastAdjuster(0.25),
                new FlaggedDeskForecastAdjuster(flags, "desk-forecast-tiered", 3.9),
                new PercentDeskForecastAdjuster(7.2),
            },
            new List<string>(),
            3.6,
            95613.0);
    }

    public double Compute(DeskForecast item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeskForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
