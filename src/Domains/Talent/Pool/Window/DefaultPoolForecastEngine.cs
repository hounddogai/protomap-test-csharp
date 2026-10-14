namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pool forecast calculation.</summary>
public interface IDefaultPoolForecastModifier
{
    string Describe();
    double Adjust(DefaultPoolForecast item, double valueValue);
}

public sealed record PercentDefaultPoolForecastAdjuster(double Percent) : IDefaultPoolForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPoolForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultPoolForecastAdjuster(double Step) : IDefaultPoolForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPoolForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDefaultPoolForecastAdjuster(double Cap) : IDefaultPoolForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPoolForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultPoolForecastAdjuster(double Minimum) : IDefaultPoolForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPoolForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultPoolForecastAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPoolForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPoolForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pool forecast steps in order and keeps a trace of each.</summary>
public sealed class DefaultPoolForecastEngine(List<IDefaultPoolForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPoolForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPoolForecastEngine DefaultDefaultPoolForecastEngine(Registry flags)
    {
        return new DefaultPoolForecastEngine(
            new List<IDefaultPoolForecastModifier>
            {
                new PercentDefaultPoolForecastAdjuster(-14.8),
                new RoundingDefaultPoolForecastAdjuster(0.5),
                new CapDefaultPoolForecastAdjuster(29389.0),
                new MinimumDefaultPoolForecastAdjuster(40.1),
                new FlaggedDefaultPoolForecastAdjuster(flags, "default-pool-forecast-default", -5.6),
            },
            new List<string>(),
            4.7,
            51520.5);
    }

    public double Compute(DefaultPoolForecast item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPoolForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
