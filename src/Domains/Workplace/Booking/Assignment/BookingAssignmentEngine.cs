namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking assignment calculation.</summary>
public interface IBookingAssignmentFactor
{
    string Describe();
    double Adjust(BookingAssignment item, double valueValue);
}

public sealed record TieredBookingAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IBookingAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingAssignment item, double valueValue)
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

public sealed record FlaggedBookingAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IBookingAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingAssignmentAdjuster(double Cap) : IBookingAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBookingAssignmentAdjuster(double Step) : IBookingAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBookingAssignmentAdjuster(double Percent) : IBookingAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking assignment steps in order and keeps a trace of each.</summary>
public sealed class BookingAssignmentEngine(List<IBookingAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingAssignmentEngine DefaultBookingAssignmentEngine(Registry flags)
    {
        return new BookingAssignmentEngine(
            new List<IBookingAssignmentFactor>
            {
                new TieredBookingAssignmentAdjuster(
                    new List<double> { 1843.0, 3953.0, 4345.0, 8261.0 },
                    new List<double> { 0.86, 0.84, 1.1, 1.19 }),
                new FlaggedBookingAssignmentAdjuster(flags, "booking-assignment-scheduled", -2.9),
                new CapBookingAssignmentAdjuster(9295.0),
                new RoundingBookingAssignmentAdjuster(0.25),
                new PercentBookingAssignmentAdjuster(-14.2),
            },
            new List<string>(),
            0.1,
            40860.0);
    }

    public double Run(BookingAssignment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
