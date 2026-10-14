namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled booking baseline calculation.</summary>
public interface IScheduledBookingBaselineModifier
{
    string Describe();
    double Adjust(ScheduledBookingBaseline item, double valueValue);
}

public sealed record PercentScheduledBookingBaselineAdjuster(double Percent) : IScheduledBookingBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBookingBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledBookingBaselineAdjuster(double Step) : IScheduledBookingBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBookingBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledBookingBaselineAdjuster(double Minimum) : IScheduledBookingBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBookingBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledBookingBaselineAdjuster(double Cap) : IScheduledBookingBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledBookingBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled booking baseline steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBookingBaselineEngine(List<IScheduledBookingBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBookingBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBookingBaselineEngine DefaultScheduledBookingBaselineEngine()
    {
        return new ScheduledBookingBaselineEngine(
            new List<IScheduledBookingBaselineModifier>
            {
                new PercentScheduledBookingBaselineAdjuster(-8.1),
                new RoundingScheduledBookingBaselineAdjuster(0.05),
                new MinimumScheduledBookingBaselineAdjuster(13.8),
                new CapScheduledBookingBaselineAdjuster(31026.0),
            },
            new List<string>(),
            4.1,
            24483.3);
    }

    public double Price(ScheduledBookingBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBookingBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
