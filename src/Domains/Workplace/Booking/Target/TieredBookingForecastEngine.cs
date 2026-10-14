namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered booking forecast calculation.</summary>
public interface ITieredBookingForecastFactor
{
    string Describe();
    double Adjust(TieredBookingForecast item, double valueValue);
}

public sealed record PercentTieredBookingForecastAdjuster(double Percent) : ITieredBookingForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredBookingForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTieredBookingForecastAdjuster(Registry Flags, string Flag, double Percent) : ITieredBookingForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredBookingForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingForecastRateOffsetAdjuster(double Weight) : ITieredBookingForecastFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(TieredBookingForecast item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record MinimumTieredBookingForecastAdjuster(double Minimum) : ITieredBookingForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBookingForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered booking forecast steps in order and keeps a trace of each.</summary>
public sealed class TieredBookingForecastEngine(List<ITieredBookingForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBookingForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBookingForecastEngine DefaultTieredBookingForecastEngine(Registry flags)
    {
        return new TieredBookingForecastEngine(
            new List<ITieredBookingForecastFactor>
            {
                new PercentTieredBookingForecastAdjuster(7.4),
                new FlaggedTieredBookingForecastAdjuster(flags, "tiered-booking-forecast-lenient", 4.0),
                new TieredBookingForecastRateOffsetAdjuster(0.93),
                new MinimumTieredBookingForecastAdjuster(0.6),
            },
            new List<string>(),
            3.4,
            49388.1);
    }

    public double Price(TieredBookingForecast item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBookingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
