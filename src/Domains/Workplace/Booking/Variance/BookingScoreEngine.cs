namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the booking score calculation.</summary>
public interface IBookingScoreModifier
{
    string Describe();
    double Adjust(BookingScore item, double valueValue);
}

public sealed record FlaggedBookingScoreAdjuster(Registry Flags, string Flag, double Percent) : IBookingScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingScoreAdjuster(List<double> Bounds, List<double> Rates) : IBookingScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingScore item, double valueValue)
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

public sealed record BookingScoreLeaveDaysOffsetAdjuster(double Weight) : IBookingScoreModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(BookingScore item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumBookingScoreAdjuster(double Minimum) : IBookingScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the booking score steps in order and keeps a trace of each.</summary>
public sealed class BookingScoreEngine(List<IBookingScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingScoreEngine DefaultBookingScoreEngine(Registry flags)
    {
        return new BookingScoreEngine(
            new List<IBookingScoreModifier>
            {
                new FlaggedBookingScoreAdjuster(flags, "booking-score-primary", -2.2),
                new TieredBookingScoreAdjuster(
                    new List<double> { 980.0, 4191.0, 9184.0, 9557.0 },
                    new List<double> { 0.89, 1.03, 1.0, 1.02 }),
                new BookingScoreLeaveDaysOffsetAdjuster(1.51),
                new MinimumBookingScoreAdjuster(29.7),
            },
            new List<string>(),
            4.2,
            80041.8);
    }

    public double Price(BookingScore item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BookingScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
