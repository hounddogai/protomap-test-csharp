namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking ledger calculation.</summary>
public interface IBookingLedgerFactor
{
    string Describe();
    double Adjust(BookingLedger item, double valueValue);
}

public sealed record BookingLedgerAttemptsOffsetAdjuster(double Weight) : IBookingLedgerFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record NameBookingLedgerAdjuster(Dictionary<string, double> Factors) : IBookingLedgerFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBookingLedgerAdjuster(double Percent) : IBookingLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBookingLedgerAdjuster(double Step) : IBookingLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBookingLedgerAdjuster(double Cap) : IBookingLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBookingLedgerAdjuster(Registry Flags, string Flag, double Percent) : IBookingLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking ledger steps in order and keeps a trace of each.</summary>
public sealed class BookingLedgerEngine(List<IBookingLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingLedgerEngine DefaultBookingLedgerEngine(Registry flags)
    {
        return new BookingLedgerEngine(
            new List<IBookingLedgerFactor>
            {
                new BookingLedgerAttemptsOffsetAdjuster(0.96),
                new NameBookingLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.16, ["central"] = 1.36, ["south"] = 1.13 }),
                new PercentBookingLedgerAdjuster(17.2),
                new RoundingBookingLedgerAdjuster(0.1),
                new CapBookingLedgerAdjuster(9231.0),
                new FlaggedBookingLedgerAdjuster(flags, "booking-ledger-manual", -4.0),
            },
            new List<string>(),
            3.3,
            5053.9);
    }

    public double Evaluate(BookingLedger item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BookingLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
