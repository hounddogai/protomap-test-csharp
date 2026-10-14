namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal booking segment calculation.</summary>
public interface ISeasonalBookingSegmentFactor
{
    string Describe();
    double Adjust(SeasonalBookingSegment item, double valueValue);
}

public sealed record MinimumSeasonalBookingSegmentAdjuster(double Minimum) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalBookingSegmentAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalBookingSegmentAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
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

public sealed record NameSeasonalBookingSegmentAdjuster(Dictionary<string, double> Factors) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalBookingSegmentAdjuster(double Percent) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalBookingSegmentExpiresAtOffsetAdjuster(double Weight) : ISeasonalBookingSegmentFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(SeasonalBookingSegment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the seasonal booking segment steps in order and keeps a trace of each.</summary>
public sealed class SeasonalBookingSegmentEngine(List<ISeasonalBookingSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalBookingSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalBookingSegmentEngine DefaultSeasonalBookingSegmentEngine(Registry flags)
    {
        return new SeasonalBookingSegmentEngine(
            new List<ISeasonalBookingSegmentFactor>
            {
                new MinimumSeasonalBookingSegmentAdjuster(10.5),
                new FlaggedSeasonalBookingSegmentAdjuster(flags, "seasonal-booking-segment-legacy", 2.6),
                new TieredSeasonalBookingSegmentAdjuster(
                    new List<double> { 6515.0, 6917.0, 8441.0, 9801.0 },
                    new List<double> { 0.87, 1.09, 0.98, 0.84 }),
                new NameSeasonalBookingSegmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["west"] = 1.34, ["south"] = 0.78 }),
                new PercentSeasonalBookingSegmentAdjuster(-3.3),
                new SeasonalBookingSegmentExpiresAtOffsetAdjuster(0.12),
            },
            new List<string>(),
            3.1,
            84533.4);
    }

    public double Compute(SeasonalBookingSegment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalBookingSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
