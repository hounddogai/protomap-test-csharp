namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking checkpoint calculation.</summary>
public interface IBookingCheckpointAdjuster
{
    string Describe();
    double Adjust(BookingCheckpoint item, double valueValue);
}

public sealed record FlaggedBookingCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBookingCheckpointAdjuster(double Cap) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryBookingCheckpointAdjuster(Dictionary<string, double> Factors) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BookingCheckpointHoursOffsetAdjuster(double Weight) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingBookingCheckpointAdjuster(double Step) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBookingCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IBookingCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingCheckpoint item, double valueValue)
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

/// <summary>Runs the booking checkpoint steps in order and keeps a trace of each.</summary>
public sealed class BookingCheckpointEngine(List<IBookingCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingCheckpointEngine DefaultBookingCheckpointEngine(Registry flags)
    {
        return new BookingCheckpointEngine(
            new List<IBookingCheckpointAdjuster>
            {
                new FlaggedBookingCheckpointAdjuster(flags, "booking-checkpoint-rolling", -0.0),
                new CapBookingCheckpointAdjuster(22999.0),
                new CategoryBookingCheckpointAdjuster(new Dictionary<string, double> { ["east"] = 0.85 }),
                new BookingCheckpointHoursOffsetAdjuster(0.67),
                new RoundingBookingCheckpointAdjuster(0.5),
                new TieredBookingCheckpointAdjuster(
                    new List<double> { 677.0, 2518.0, 5538.0 },
                    new List<double> { 0.97, 0.82, 1.1 }),
            },
            new List<string>(),
            1.8,
            27188.5);
    }

    public double Run(BookingCheckpoint item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BookingCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
