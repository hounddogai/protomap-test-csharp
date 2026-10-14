namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking profile calculation.</summary>
public interface IBookingProfileAdjuster
{
    string Describe();
    double Adjust(BookingProfile item, double valueValue);
}

public sealed record TieredBookingProfileAdjuster(List<double> Bounds, List<double> Rates) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingProfile item, double valueValue)
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

public sealed record FlaggedBookingProfileAdjuster(Registry Flags, string Flag, double Percent) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBookingProfileAdjuster(double Minimum) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBookingProfileAdjuster(double Cap) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBookingProfileAdjuster(double Percent) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBookingProfileAdjuster(double Step) : IBookingProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the booking profile steps in order and keeps a trace of each.</summary>
public sealed class BookingProfileEngine(List<IBookingProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingProfileEngine DefaultBookingProfileEngine(Registry flags)
    {
        return new BookingProfileEngine(
            new List<IBookingProfileAdjuster>
            {
                new TieredBookingProfileAdjuster(
                    new List<double> { 48.0, 112.0, 1604.0, 7201.0 },
                    new List<double> { 0.97, 0.89, 0.87, 1.04 }),
                new FlaggedBookingProfileAdjuster(flags, "booking-profile-regional", -7.3),
                new MinimumBookingProfileAdjuster(15.5),
                new CapBookingProfileAdjuster(27084.0),
                new PercentBookingProfileAdjuster(-14.9),
                new RoundingBookingProfileAdjuster(5.0),
            },
            new List<string>(),
            1.4,
            77020.0);
    }

    public double Price(BookingProfile item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BookingProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
