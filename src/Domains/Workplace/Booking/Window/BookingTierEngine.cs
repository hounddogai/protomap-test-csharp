namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking tier calculation.</summary>
public interface IBookingTierFactor
{
    string Describe();
    double Adjust(BookingTier item, double valueValue);
}

public sealed record NameBookingTierAdjuster(Dictionary<string, double> Factors) : IBookingTierFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BookingTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBookingTierAdjuster(double Step) : IBookingTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBookingTierAdjuster(Registry Flags, string Flag, double Percent) : IBookingTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingTierAdjuster(double Cap) : IBookingTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the booking tier steps in order and keeps a trace of each.</summary>
public sealed class BookingTierEngine(List<IBookingTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingTierEngine DefaultBookingTierEngine(Registry flags)
    {
        return new BookingTierEngine(
            new List<IBookingTierFactor>
            {
                new NameBookingTierAdjuster(new Dictionary<string, double> { ["south"] = 0.79 }),
                new RoundingBookingTierAdjuster(5.0),
                new FlaggedBookingTierAdjuster(flags, "booking-tier-global", -3.4),
                new CapBookingTierAdjuster(47282.0),
            },
            new List<string>(),
            0.3,
            97016.2);
    }

    public double Compute(BookingTier item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
