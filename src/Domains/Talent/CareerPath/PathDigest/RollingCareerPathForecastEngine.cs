namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling career path forecast calculation.</summary>
public interface IRollingCareerPathForecastFactor
{
    string Describe();
    double Adjust(RollingCareerPathForecast item, double valueValue);
}

public sealed record MinimumRollingCareerPathForecastAdjuster(double Minimum) : IRollingCareerPathForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingCareerPathForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRollingCareerPathForecastAdjuster(double Step) : IRollingCareerPathForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingCareerPathForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRollingCareerPathForecastAdjuster(double Cap) : IRollingCareerPathForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingCareerPathForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling career path forecast steps in order and keeps a trace of each.</summary>
public sealed class RollingCareerPathForecastEngine(List<IRollingCareerPathForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingCareerPathForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingCareerPathForecastEngine DefaultRollingCareerPathForecastEngine()
    {
        return new RollingCareerPathForecastEngine(
            new List<IRollingCareerPathForecastFactor>
            {
                new MinimumRollingCareerPathForecastAdjuster(48.5),
                new RoundingRollingCareerPathForecastAdjuster(0.05),
                new CapRollingCareerPathForecastAdjuster(6789.0),
            },
            new List<string>(),
            3.5,
            73485.5);
    }

    public double Run(RollingCareerPathForecast item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingCareerPathForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
