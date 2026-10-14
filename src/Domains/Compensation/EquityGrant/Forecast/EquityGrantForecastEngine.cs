namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant forecast calculation.</summary>
public interface IEquityGrantForecastAdjuster
{
    string Describe();
    double Adjust(EquityGrantForecast item, double valueValue);
}

public sealed record MinimumEquityGrantForecastAdjuster(double Minimum) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record EquityGrantForecastLimitOffsetAdjuster(double Weight) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapEquityGrantForecastAdjuster(double Cap) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentEquityGrantForecastAdjuster(double Percent) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEquityGrantForecastAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
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

public sealed record FlaggedEquityGrantForecastAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the equity grant forecast steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantForecastEngine(List<IEquityGrantForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantForecastEngine DefaultEquityGrantForecastEngine(Registry flags)
    {
        return new EquityGrantForecastEngine(
            new List<IEquityGrantForecastAdjuster>
            {
                new MinimumEquityGrantForecastAdjuster(27.8),
                new EquityGrantForecastLimitOffsetAdjuster(0.85),
                new CapEquityGrantForecastAdjuster(8140.0),
                new PercentEquityGrantForecastAdjuster(-2.7),
                new TieredEquityGrantForecastAdjuster(
                    new List<double> { 4165.0, 6690.0, 7965.0, 9248.0 },
                    new List<double> { 1.18, 1.0, 0.83, 0.82 }),
                new FlaggedEquityGrantForecastAdjuster(flags, "equity-grant-forecast-legacy", 4.3),
            },
            new List<string>(),
            0.1,
            31431.8);
    }

    public double Compute(EquityGrantForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
