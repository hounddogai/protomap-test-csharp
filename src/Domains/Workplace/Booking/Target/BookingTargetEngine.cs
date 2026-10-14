namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking target calculation.</summary>
public interface IBookingTargetAdjuster
{
    string Describe();
    double Adjust(BookingTarget item, double valueValue);
}

public sealed record RegionBookingTargetAdjuster(Dictionary<string, double> Factors) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(BookingTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BookingTargetTenureMonthsOffsetAdjuster(double Weight) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(BookingTarget item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedBookingTargetAdjuster(Registry Flags, string Flag, double Percent) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBookingTargetAdjuster(double Percent) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBookingTargetAdjuster(double Minimum) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBookingTargetAdjuster(List<double> Bounds, List<double> Rates) : IBookingTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingTarget item, double valueValue)
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

/// <summary>Runs the booking target steps in order and keeps a trace of each.</summary>
public sealed class BookingTargetEngine(List<IBookingTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingTargetEngine DefaultBookingTargetEngine(Registry flags)
    {
        return new BookingTargetEngine(
            new List<IBookingTargetAdjuster>
            {
                new RegionBookingTargetAdjuster(new Dictionary<string, double> { ["west"] = 0.87, ["east"] = 0.8 }),
                new BookingTargetTenureMonthsOffsetAdjuster(1.17),
                new FlaggedBookingTargetAdjuster(flags, "booking-target-tiered", -3.2),
                new PercentBookingTargetAdjuster(-12.1),
                new MinimumBookingTargetAdjuster(12.3),
                new TieredBookingTargetAdjuster(new List<double> { 2628.0, 5885.0 }, new List<double> { 1.13, 0.92 }),
            },
            new List<string>(),
            0.1,
            84557.0);
    }

    public double Run(BookingTarget item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
