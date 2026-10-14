namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span forecast calculation.</summary>
public interface ISpanForecastFactor
{
    string Describe();
    double Adjust(SpanForecast item, double valueValue);
}

public sealed record FlaggedSpanForecastAdjuster(Registry Flags, string Flag, double Percent) : ISpanForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSpanForecastAdjuster(double Percent) : ISpanForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanForecastAdjuster(double Minimum) : ISpanForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the span forecast steps in order and keeps a trace of each.</summary>
public sealed class SpanForecastEngine(List<ISpanForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanForecastEngine DefaultSpanForecastEngine(Registry flags)
    {
        return new SpanForecastEngine(
            new List<ISpanForecastFactor>
            {
                new FlaggedSpanForecastAdjuster(flags, "span-forecast-regional", -2.8),
                new PercentSpanForecastAdjuster(-8.0),
                new MinimumSpanForecastAdjuster(7.2),
            },
            new List<string>(),
            2.1,
            37445.4);
    }

    public double Run(SpanForecast item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
