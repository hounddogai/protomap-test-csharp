namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking entry calculation.</summary>
public interface IBookingEntryStep
{
    string Describe();
    double Adjust(BookingEntry item, double valueValue);
}

public sealed record SegmentBookingEntryAdjuster(Dictionary<string, double> Factors) : IBookingEntryStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BookingEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBookingEntryAdjuster(double Step) : IBookingEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBookingEntryAdjuster(double Percent) : IBookingEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingEntryAdjuster(List<double> Bounds, List<double> Rates) : IBookingEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingEntry item, double valueValue)
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

public sealed record FlaggedBookingEntryAdjuster(Registry Flags, string Flag, double Percent) : IBookingEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking entry steps in order and keeps a trace of each.</summary>
public sealed class BookingEntryEngine(List<IBookingEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingEntryEngine DefaultBookingEntryEngine(Registry flags)
    {
        return new BookingEntryEngine(
            new List<IBookingEntryStep>
            {
                new SegmentBookingEntryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.23, ["west"] = 1.12, ["east"] = 0.92 }),
                new RoundingBookingEntryAdjuster(0.5),
                new PercentBookingEntryAdjuster(22.1),
                new TieredBookingEntryAdjuster(
                    new List<double> { 2538.0, 3949.0, 5351.0 },
                    new List<double> { 0.9, 1.03, 1.1 }),
                new FlaggedBookingEntryAdjuster(flags, "booking-entry-lenient", -3.8),
            },
            new List<string>(),
            1.1,
            71749.9);
    }

    public double Compute(BookingEntry item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BookingEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
