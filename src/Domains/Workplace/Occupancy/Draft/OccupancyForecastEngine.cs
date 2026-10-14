namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy forecast calculation.</summary>
public interface IOccupancyForecastFactor
{
    string Describe();
    double Adjust(OccupancyForecast item, double valueValue);
}

public sealed record OccupancyForecastExpiresAtOffsetAdjuster(double Weight) : IOccupancyForecastFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(OccupancyForecast item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingOccupancyForecastAdjuster(double Step) : IOccupancyForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapOccupancyForecastAdjuster(double Cap) : IOccupancyForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumOccupancyForecastAdjuster(double Minimum) : IOccupancyForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the occupancy forecast steps in order and keeps a trace of each.</summary>
public sealed class OccupancyForecastEngine(List<IOccupancyForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyForecastEngine DefaultOccupancyForecastEngine()
    {
        return new OccupancyForecastEngine(
            new List<IOccupancyForecastFactor>
            {
                new OccupancyForecastExpiresAtOffsetAdjuster(0.35),
                new RoundingOccupancyForecastAdjuster(0.1),
                new CapOccupancyForecastAdjuster(43283.0),
                new MinimumOccupancyForecastAdjuster(49.9),
            },
            new List<string>(),
            2.8,
            26406.4);
    }

    public double Evaluate(OccupancyForecast item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
