namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking agreement calculation.</summary>
public interface IBookingAgreementStep
{
    string Describe();
    double Adjust(BookingAgreement item, double valueValue);
}

public sealed record MinimumBookingAgreementAdjuster(double Minimum) : IBookingAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBookingAgreementAdjuster(double Percent) : IBookingAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBookingAgreementAdjuster(double Step) : IBookingAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBookingAgreementAdjuster(Registry Flags, string Flag, double Percent) : IBookingAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingAgreementAdjuster(List<double> Bounds, List<double> Rates) : IBookingAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingAgreement item, double valueValue)
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

/// <summary>Runs the booking agreement steps in order and keeps a trace of each.</summary>
public sealed class BookingAgreementEngine(List<IBookingAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingAgreementEngine DefaultBookingAgreementEngine(Registry flags)
    {
        return new BookingAgreementEngine(
            new List<IBookingAgreementStep>
            {
                new MinimumBookingAgreementAdjuster(35.0),
                new PercentBookingAgreementAdjuster(16.8),
                new RoundingBookingAgreementAdjuster(1.0),
                new FlaggedBookingAgreementAdjuster(flags, "booking-agreement-lenient", -1.9),
                new TieredBookingAgreementAdjuster(
                    new List<double> { 2174.0, 6203.0 },
                    new List<double> { 0.83, 0.96 }),
            },
            new List<string>(),
            0.8,
            77757.0);
    }

    public double Evaluate(BookingAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<BookingAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
