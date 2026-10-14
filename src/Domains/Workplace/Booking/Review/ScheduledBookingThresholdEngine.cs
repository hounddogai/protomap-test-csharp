namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled booking threshold calculation.</summary>
public interface IScheduledBookingThresholdAdjuster
{
    string Describe();
    double Adjust(ScheduledBookingThreshold item, double valueValue);
}

public sealed record PercentScheduledBookingThresholdAdjuster(double Percent) : IScheduledBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBookingThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledBookingThresholdAdjuster(double Minimum) : IScheduledBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBookingThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledBookingThresholdAdjuster(List<double> Bounds, List<double> Rates) : IScheduledBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledBookingThreshold item, double valueValue)
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

public sealed record FlaggedScheduledBookingThresholdAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBookingThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled booking threshold steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBookingThresholdEngine(List<IScheduledBookingThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBookingThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBookingThresholdEngine DefaultScheduledBookingThresholdEngine(Registry flags)
    {
        return new ScheduledBookingThresholdEngine(
            new List<IScheduledBookingThresholdAdjuster>
            {
                new PercentScheduledBookingThresholdAdjuster(18.4),
                new MinimumScheduledBookingThresholdAdjuster(19.6),
                new TieredScheduledBookingThresholdAdjuster(
                    new List<double> { 686.0, 1277.0, 7081.0, 7295.0 },
                    new List<double> { 1.04, 1.07, 1.18, 1.05 }),
                new FlaggedScheduledBookingThresholdAdjuster(flags, "scheduled-booking-threshold-primary", 1.8),
            },
            new List<string>(),
            1.3,
            28619.0);
    }

    public double Price(ScheduledBookingThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBookingThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
