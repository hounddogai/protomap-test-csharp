namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking request calculation.</summary>
public interface IBookingRequestModifier
{
    string Describe();
    double Adjust(BookingRequest item, double valueValue);
}

public sealed record CapBookingRequestAdjuster(double Cap) : IBookingRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBookingRequestAdjuster(double Percent) : IBookingRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBookingRequestAdjuster(Registry Flags, string Flag, double Percent) : IBookingRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingRequestAdjuster(List<double> Bounds, List<double> Rates) : IBookingRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingRequest item, double valueValue)
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

public sealed record RoundingBookingRequestAdjuster(double Step) : IBookingRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the booking request steps in order and keeps a trace of each.</summary>
public sealed class BookingRequestEngine(List<IBookingRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingRequestEngine DefaultBookingRequestEngine(Registry flags)
    {
        return new BookingRequestEngine(
            new List<IBookingRequestModifier>
            {
                new CapBookingRequestAdjuster(46687.0),
                new PercentBookingRequestAdjuster(-22.8),
                new FlaggedBookingRequestAdjuster(flags, "booking-request-manual", 0.4),
                new TieredBookingRequestAdjuster(new List<double> { 217.0, 4759.0 }, new List<double> { 0.84, 0.94 }),
                new RoundingBookingRequestAdjuster(5.0),
            },
            new List<string>(),
            2.6,
            58157.8);
    }

    public double Run(BookingRequest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
