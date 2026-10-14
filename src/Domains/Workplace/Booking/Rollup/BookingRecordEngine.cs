namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking record calculation.</summary>
public interface IBookingRecordFactor
{
    string Describe();
    double Adjust(BookingRecord item, double valueValue);
}

public sealed record TieredBookingRecordAdjuster(List<double> Bounds, List<double> Rates) : IBookingRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingRecord item, double valueValue)
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

public sealed record CapBookingRecordAdjuster(double Cap) : IBookingRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBookingRecordAdjuster(double Step) : IBookingRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBookingRecordAdjuster(double Percent) : IBookingRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BookingRecordMarginOffsetAdjuster(double Weight) : IBookingRecordFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(BookingRecord item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the booking record steps in order and keeps a trace of each.</summary>
public sealed class BookingRecordEngine(List<IBookingRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingRecordEngine DefaultBookingRecordEngine()
    {
        return new BookingRecordEngine(
            new List<IBookingRecordFactor>
            {
                new TieredBookingRecordAdjuster(
                    new List<double> { 228.0, 8473.0, 8518.0 },
                    new List<double> { 0.92, 1.16, 1.0 }),
                new CapBookingRecordAdjuster(14011.0),
                new RoundingBookingRecordAdjuster(0.5),
                new PercentBookingRecordAdjuster(18.8),
                new BookingRecordMarginOffsetAdjuster(1.4),
            },
            new List<string>(),
            0.8,
            36195.4);
    }

    public double Run(BookingRecord item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
