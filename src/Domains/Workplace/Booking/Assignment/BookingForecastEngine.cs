namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking forecast calculation.</summary>
public interface IBookingForecastModifier
{
    string Describe();
    double Adjust(BookingForecast item, double valueValue);
}

public sealed record TitleBookingForecastAdjuster(Dictionary<string, double> Factors) : IBookingForecastModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BookingForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBookingForecastAdjuster(double Minimum) : IBookingForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBookingForecastAdjuster(double Percent) : IBookingForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking forecast steps in order and keeps a trace of each.</summary>
public sealed class BookingForecastEngine(List<IBookingForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingForecastEngine DefaultBookingForecastEngine()
    {
        return new BookingForecastEngine(
            new List<IBookingForecastModifier>
            {
                new TitleBookingForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.18, ["south"] = 1.27, ["west"] = 0.77 }),
                new MinimumBookingForecastAdjuster(15.9),
                new PercentBookingForecastAdjuster(24.2),
            },
            new List<string>(),
            2.5,
            35928.3);
    }

    public double Evaluate(BookingForecast item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BookingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
