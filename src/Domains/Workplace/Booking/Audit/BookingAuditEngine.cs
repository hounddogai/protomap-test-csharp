namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking audit calculation.</summary>
public interface IBookingAuditAdjuster
{
    string Describe();
    double Adjust(BookingAudit item, double valueValue);
}

public sealed record CapBookingAuditAdjuster(double Cap) : IBookingAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameBookingAuditAdjuster(Dictionary<string, double> Factors) : IBookingAuditAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BookingAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBookingAuditAdjuster(double Minimum) : IBookingAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BookingAuditRateOffsetAdjuster(double Weight) : IBookingAuditAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(BookingAudit item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the booking audit steps in order and keeps a trace of each.</summary>
public sealed class BookingAuditEngine(List<IBookingAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingAuditEngine DefaultBookingAuditEngine()
    {
        return new BookingAuditEngine(
            new List<IBookingAuditAdjuster>
            {
                new CapBookingAuditAdjuster(23259.0),
                new NameBookingAuditAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.95, ["north"] = 1.2, ["west"] = 0.89 }),
                new MinimumBookingAuditAdjuster(17.0),
                new BookingAuditRateOffsetAdjuster(0.83),
            },
            new List<string>(),
            1.2,
            39803.0);
    }

    public double Price(BookingAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<BookingAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
