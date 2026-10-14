namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking adjustment calculation.</summary>
public interface IBookingAdjustmentStep
{
    string Describe();
    double Adjust(BookingAdjustment item, double valueValue);
}

public sealed record PercentBookingAdjustmentAdjuster(double Percent) : IBookingAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBookingAdjustmentAdjuster(Dictionary<string, double> Factors) : IBookingAdjustmentStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BookingAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBookingAdjustmentAdjuster(double Step) : IBookingAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBookingAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IBookingAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingAdjustment item, double valueValue)
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

/// <summary>Runs the booking adjustment steps in order and keeps a trace of each.</summary>
public sealed class BookingAdjustmentEngine(List<IBookingAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingAdjustmentEngine DefaultBookingAdjustmentEngine()
    {
        return new BookingAdjustmentEngine(
            new List<IBookingAdjustmentStep>
            {
                new PercentBookingAdjustmentAdjuster(-24.2),
                new SegmentBookingAdjustmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.94, ["west"] = 1.04, ["online"] = 0.75 }),
                new RoundingBookingAdjustmentAdjuster(0.05),
                new TieredBookingAdjustmentAdjuster(
                    new List<double> { 2129.0, 2595.0, 5775.0 },
                    new List<double> { 1.0, 0.97, 1.07 }),
            },
            new List<string>(),
            4.1,
            57524.8);
    }

    public double Run(BookingAdjustment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
