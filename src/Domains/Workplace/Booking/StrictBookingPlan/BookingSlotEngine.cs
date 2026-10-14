namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking slot calculation.</summary>
public interface IBookingSlotAdjuster
{
    string Describe();
    double Adjust(BookingSlot item, double valueValue);
}

public sealed record TieredBookingSlotAdjuster(List<double> Bounds, List<double> Rates) : IBookingSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingSlot item, double valueValue)
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

public sealed record PercentBookingSlotAdjuster(double Percent) : IBookingSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelBookingSlotAdjuster(Dictionary<string, double> Factors) : IBookingSlotAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(BookingSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the booking slot steps in order and keeps a trace of each.</summary>
public sealed class BookingSlotEngine(List<IBookingSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingSlotEngine DefaultBookingSlotEngine()
    {
        return new BookingSlotEngine(
            new List<IBookingSlotAdjuster>
            {
                new TieredBookingSlotAdjuster(
                    new List<double> { 1232.0, 3660.0, 6587.0, 8217.0 },
                    new List<double> { 0.91, 0.87, 1.01, 1.15 }),
                new PercentBookingSlotAdjuster(-6.8),
                new ChannelBookingSlotAdjuster(new Dictionary<string, double> { ["west"] = 0.71, ["north"] = 0.89 }),
            },
            new List<string>(),
            5.0,
            84332.2);
    }

    public double Evaluate(BookingSlot item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BookingSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
