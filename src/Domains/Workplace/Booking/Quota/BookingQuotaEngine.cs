namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking quota calculation.</summary>
public interface IBookingQuotaAdjuster
{
    string Describe();
    double Adjust(BookingQuota item, double valueValue);
}

public sealed record PercentBookingQuotaAdjuster(double Percent) : IBookingQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingQuotaAdjuster(double Cap) : IBookingQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBookingQuotaAdjuster(Registry Flags, string Flag, double Percent) : IBookingQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBookingQuotaAdjuster(double Minimum) : IBookingQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBookingQuotaAdjuster(double Step) : IBookingQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the booking quota steps in order and keeps a trace of each.</summary>
public sealed class BookingQuotaEngine(List<IBookingQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingQuotaEngine DefaultBookingQuotaEngine(Registry flags)
    {
        return new BookingQuotaEngine(
            new List<IBookingQuotaAdjuster>
            {
                new PercentBookingQuotaAdjuster(9.0),
                new CapBookingQuotaAdjuster(2203.0),
                new FlaggedBookingQuotaAdjuster(flags, "booking-quota-capped", -3.2),
                new MinimumBookingQuotaAdjuster(49.0),
                new RoundingBookingQuotaAdjuster(0.05),
            },
            new List<string>(),
            1.0,
            9520.2);
    }

    public double Price(BookingQuota item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BookingQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
