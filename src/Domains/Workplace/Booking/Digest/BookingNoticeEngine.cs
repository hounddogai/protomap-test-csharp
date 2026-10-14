namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking notice calculation.</summary>
public interface IBookingNoticeFactor
{
    string Describe();
    double Adjust(BookingNotice item, double valueValue);
}

public sealed record FlaggedBookingNoticeAdjuster(Registry Flags, string Flag, double Percent) : IBookingNoticeFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingNoticeAdjuster(double Cap) : IBookingNoticeFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBookingNoticeAdjuster(double Step) : IBookingNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentBookingNoticeAdjuster(Dictionary<string, double> Factors) : IBookingNoticeFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BookingNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the booking notice steps in order and keeps a trace of each.</summary>
public sealed class BookingNoticeEngine(List<IBookingNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingNoticeEngine DefaultBookingNoticeEngine(Registry flags)
    {
        return new BookingNoticeEngine(
            new List<IBookingNoticeFactor>
            {
                new FlaggedBookingNoticeAdjuster(flags, "booking-notice-regional", -0.3),
                new CapBookingNoticeAdjuster(40490.0),
                new RoundingBookingNoticeAdjuster(0.25),
                new SegmentBookingNoticeAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.98, ["online"] = 1.23, ["retail"] = 0.9 }),
            },
            new List<string>(),
            2.6,
            76111.3);
    }

    public double Run(BookingNotice item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BookingNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
