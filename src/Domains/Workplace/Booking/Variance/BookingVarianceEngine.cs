namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking variance calculation.</summary>
public interface IBookingVarianceStep
{
    string Describe();
    double Adjust(BookingVariance item, double valueValue);
}

public sealed record BookingVarianceHoursOffsetAdjuster(double Weight) : IBookingVarianceStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BookingVariance item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapBookingVarianceAdjuster(double Cap) : IBookingVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBookingVarianceAdjuster(double Step) : IBookingVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBookingVarianceAdjuster(List<double> Bounds, List<double> Rates) : IBookingVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingVariance item, double valueValue)
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

public sealed record FlaggedBookingVarianceAdjuster(Registry Flags, string Flag, double Percent) : IBookingVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking variance steps in order and keeps a trace of each.</summary>
public sealed class BookingVarianceEngine(List<IBookingVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingVarianceEngine DefaultBookingVarianceEngine(Registry flags)
    {
        return new BookingVarianceEngine(
            new List<IBookingVarianceStep>
            {
                new BookingVarianceHoursOffsetAdjuster(0.21),
                new CapBookingVarianceAdjuster(27728.0),
                new RoundingBookingVarianceAdjuster(5.0),
                new TieredBookingVarianceAdjuster(new List<double> { 647.0, 8967.0 }, new List<double> { 0.91, 1.2 }),
                new FlaggedBookingVarianceAdjuster(flags, "booking-variance-strict", 1.5),
            },
            new List<string>(),
            0.2,
            59056.3);
    }

    public double Run(BookingVariance item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BookingVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
