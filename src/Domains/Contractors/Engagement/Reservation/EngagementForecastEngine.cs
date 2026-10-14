namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement forecast calculation.</summary>
public interface IEngagementForecastFactor
{
    string Describe();
    double Adjust(EngagementForecast item, double valueValue);
}

public sealed record EngagementForecastExpiresAtOffsetAdjuster(double Weight) : IEngagementForecastFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(EngagementForecast item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record LabelEngagementForecastAdjuster(Dictionary<string, double> Factors) : IEngagementForecastFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapEngagementForecastAdjuster(double Cap) : IEngagementForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the engagement forecast steps in order and keeps a trace of each.</summary>
public sealed class EngagementForecastEngine(List<IEngagementForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementForecastEngine DefaultEngagementForecastEngine()
    {
        return new EngagementForecastEngine(
            new List<IEngagementForecastFactor>
            {
                new EngagementForecastExpiresAtOffsetAdjuster(1.17),
                new LabelEngagementForecastAdjuster(new Dictionary<string, double> { ["south"] = 0.87 }),
                new CapEngagementForecastAdjuster(46595.0),
            },
            new List<string>(),
            0.6,
            63762.6);
    }

    public double Price(EngagementForecast item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
