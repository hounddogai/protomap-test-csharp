namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking digest calculation.</summary>
public interface IBookingDigestFactor
{
    string Describe();
    double Adjust(BookingDigest item, double valueValue);
}

public sealed record FlaggedBookingDigestAdjuster(Registry Flags, string Flag, double Percent) : IBookingDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingDigestAdjuster(double Cap) : IBookingDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBookingDigestAdjuster(List<double> Bounds, List<double> Rates) : IBookingDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingDigest item, double valueValue)
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

/// <summary>Runs the booking digest steps in order and keeps a trace of each.</summary>
public sealed class BookingDigestEngine(List<IBookingDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingDigestEngine DefaultBookingDigestEngine(Registry flags)
    {
        return new BookingDigestEngine(
            new List<IBookingDigestFactor>
            {
                new FlaggedBookingDigestAdjuster(flags, "booking-digest-capped", -9.0),
                new CapBookingDigestAdjuster(31854.0),
                new TieredBookingDigestAdjuster(
                    new List<double> { 2279.0, 6497.0, 8479.0, 9645.0 },
                    new List<double> { 0.91, 0.82, 0.85, 0.86 }),
            },
            new List<string>(),
            3.6,
            66447.8);
    }

    public double Evaluate(BookingDigest item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BookingDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
