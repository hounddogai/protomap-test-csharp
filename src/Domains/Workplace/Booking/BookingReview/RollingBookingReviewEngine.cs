namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling booking review calculation.</summary>
public interface IRollingBookingReviewStep
{
    string Describe();
    double Adjust(RollingBookingReview item, double valueValue);
}

public sealed record MinimumRollingBookingReviewAdjuster(double Minimum) : IRollingBookingReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingBookingReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRollingBookingReviewAdjuster(double Percent) : IRollingBookingReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingBookingReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingBookingReviewAdjuster(Registry Flags, string Flag, double Percent) : IRollingBookingReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBookingReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingBookingReviewWeightOffsetAdjuster(double Weight) : IRollingBookingReviewStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RollingBookingReview item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingRollingBookingReviewAdjuster(double Step) : IRollingBookingReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingBookingReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling booking review steps in order and keeps a trace of each.</summary>
public sealed class RollingBookingReviewEngine(List<IRollingBookingReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBookingReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBookingReviewEngine DefaultRollingBookingReviewEngine(Registry flags)
    {
        return new RollingBookingReviewEngine(
            new List<IRollingBookingReviewStep>
            {
                new MinimumRollingBookingReviewAdjuster(42.0),
                new PercentRollingBookingReviewAdjuster(-14.1),
                new FlaggedRollingBookingReviewAdjuster(flags, "rolling-booking-review-rolling", -7.3),
                new RollingBookingReviewWeightOffsetAdjuster(0.46),
                new RoundingRollingBookingReviewAdjuster(0.25),
            },
            new List<string>(),
            0.9,
            53800.8);
    }

    public double Price(RollingBookingReview item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBookingReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
