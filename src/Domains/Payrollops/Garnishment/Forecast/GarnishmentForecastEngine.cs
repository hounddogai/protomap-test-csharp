namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment forecast calculation.</summary>
public interface IGarnishmentForecastFactor
{
    string Describe();
    double Adjust(GarnishmentForecast item, double valueValue);
}

public sealed record PercentGarnishmentForecastAdjuster(double Percent) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGarnishmentForecastAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentForecastAdjuster(double Cap) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleGarnishmentForecastAdjuster(Dictionary<string, double> Factors) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGarnishmentForecastAdjuster(double Step) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGarnishmentForecastAdjuster(double Minimum) : IGarnishmentForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the garnishment forecast steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentForecastEngine(List<IGarnishmentForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentForecastEngine DefaultGarnishmentForecastEngine(Registry flags)
    {
        return new GarnishmentForecastEngine(
            new List<IGarnishmentForecastFactor>
            {
                new PercentGarnishmentForecastAdjuster(6.4),
                new FlaggedGarnishmentForecastAdjuster(flags, "garnishment-forecast-seasonal", -2.3),
                new CapGarnishmentForecastAdjuster(39883.0),
                new LocaleGarnishmentForecastAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.75, ["south"] = 1.33 }),
                new RoundingGarnishmentForecastAdjuster(0.1),
                new MinimumGarnishmentForecastAdjuster(28.0),
            },
            new List<string>(),
            0.3,
            49900.9);
    }

    public double Compute(GarnishmentForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
