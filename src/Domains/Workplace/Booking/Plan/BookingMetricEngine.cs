namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking metric calculation.</summary>
public interface IBookingMetricAdjuster
{
    string Describe();
    double Adjust(BookingMetric item, double valueValue);
}

public sealed record MinimumBookingMetricAdjuster(double Minimum) : IBookingMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBookingMetricAdjuster(double Cap) : IBookingMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBookingMetricAdjuster(List<double> Bounds, List<double> Rates) : IBookingMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingMetric item, double valueValue)
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

public sealed record CategoryBookingMetricAdjuster(Dictionary<string, double> Factors) : IBookingMetricAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BookingMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the booking metric steps in order and keeps a trace of each.</summary>
public sealed class BookingMetricEngine(List<IBookingMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingMetricEngine DefaultBookingMetricEngine()
    {
        return new BookingMetricEngine(
            new List<IBookingMetricAdjuster>
            {
                new MinimumBookingMetricAdjuster(40.4),
                new CapBookingMetricAdjuster(26004.0),
                new TieredBookingMetricAdjuster(new List<double> { 676.0, 7210.0 }, new List<double> { 0.95, 1.16 }),
                new CategoryBookingMetricAdjuster(new Dictionary<string, double> { ["north"] = 1.11 }),
            },
            new List<string>(),
            3.3,
            7211.2);
    }

    public double Price(BookingMetric item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BookingMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
