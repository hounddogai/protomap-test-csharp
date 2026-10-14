namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility forecast calculation.</summary>
public interface IMobilityForecastStep
{
    string Describe();
    double Adjust(MobilityForecast item, double valueValue);
}

public sealed record FlaggedMobilityForecastAdjuster(Registry Flags, string Flag, double Percent) : IMobilityForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilityForecastAdjuster(double Cap) : IMobilityForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMobilityForecastAdjuster(double Minimum) : IMobilityForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the mobility forecast steps in order and keeps a trace of each.</summary>
public sealed class MobilityForecastEngine(List<IMobilityForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityForecastEngine DefaultMobilityForecastEngine(Registry flags)
    {
        return new MobilityForecastEngine(
            new List<IMobilityForecastStep>
            {
                new FlaggedMobilityForecastAdjuster(flags, "mobility-forecast-manual", -5.9),
                new CapMobilityForecastAdjuster(20162.0),
                new MinimumMobilityForecastAdjuster(34.9),
            },
            new List<string>(),
            2.2,
            52954.2);
    }

    public double Evaluate(MobilityForecast item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
