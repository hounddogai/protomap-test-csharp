namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual booking assignment calculation.</summary>
public interface IManualBookingAssignmentModifier
{
    string Describe();
    double Adjust(ManualBookingAssignment item, double valueValue);
}

public sealed record RoundingManualBookingAssignmentAdjuster(double Step) : IManualBookingAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBookingAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualBookingAssignmentAdjuster(double Minimum) : IManualBookingAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualBookingAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ManualBookingAssignmentWeightOffsetAdjuster(double Weight) : IManualBookingAssignmentModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ManualBookingAssignment item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedManualBookingAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IManualBookingAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualBookingAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualBookingAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IManualBookingAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBookingAssignment item, double valueValue)
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

/// <summary>Runs the manual booking assignment steps in order and keeps a trace of each.</summary>
public sealed class ManualBookingAssignmentEngine(List<IManualBookingAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBookingAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBookingAssignmentEngine DefaultManualBookingAssignmentEngine(Registry flags)
    {
        return new ManualBookingAssignmentEngine(
            new List<IManualBookingAssignmentModifier>
            {
                new RoundingManualBookingAssignmentAdjuster(0.25),
                new MinimumManualBookingAssignmentAdjuster(20.9),
                new ManualBookingAssignmentWeightOffsetAdjuster(0.08),
                new FlaggedManualBookingAssignmentAdjuster(flags, "manual-booking-assignment-strict", 9.4),
                new TieredManualBookingAssignmentAdjuster(
                    new List<double> { 2848.0, 6381.0, 6969.0, 7050.0 },
                    new List<double> { 1.08, 1.14, 0.96, 1.13 }),
            },
            new List<string>(),
            3.1,
            78457.4);
    }

    public double Compute(ManualBookingAssignment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBookingAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
