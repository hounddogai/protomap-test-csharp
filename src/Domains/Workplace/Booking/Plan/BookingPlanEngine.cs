namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking plan calculation.</summary>
public interface IBookingPlanModifier
{
    string Describe();
    double Adjust(BookingPlan item, double valueValue);
}

public sealed record TieredBookingPlanAdjuster(List<double> Bounds, List<double> Rates) : IBookingPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingPlan item, double valueValue)
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

public sealed record NameBookingPlanAdjuster(Dictionary<string, double> Factors) : IBookingPlanModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BookingPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBookingPlanAdjuster(Registry Flags, string Flag, double Percent) : IBookingPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking plan steps in order and keeps a trace of each.</summary>
public sealed class BookingPlanEngine(List<IBookingPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingPlanEngine DefaultBookingPlanEngine(Registry flags)
    {
        return new BookingPlanEngine(
            new List<IBookingPlanModifier>
            {
                new TieredBookingPlanAdjuster(
                    new List<double> { 469.0, 1119.0, 8017.0 },
                    new List<double> { 1.07, 1.07, 0.93 }),
                new NameBookingPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.4, ["central"] = 0.82, ["retail"] = 0.77 }),
                new FlaggedBookingPlanAdjuster(flags, "booking-plan-manual", -0.2),
            },
            new List<string>(),
            1.8,
            24685.6);
    }

    public double Price(BookingPlan item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BookingPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
