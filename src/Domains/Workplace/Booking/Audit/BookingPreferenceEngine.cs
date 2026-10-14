namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking preference calculation.</summary>
public interface IBookingPreferenceModifier
{
    string Describe();
    double Adjust(BookingPreference item, double valueValue);
}

public sealed record CapBookingPreferenceAdjuster(double Cap) : IBookingPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBookingPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IBookingPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBookingPreferenceAdjuster(double Step) : IBookingPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelBookingPreferenceAdjuster(Dictionary<string, double> Factors) : IBookingPreferenceModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BookingPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBookingPreferenceAdjuster(double Minimum) : IBookingPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the booking preference steps in order and keeps a trace of each.</summary>
public sealed class BookingPreferenceEngine(List<IBookingPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingPreferenceEngine DefaultBookingPreferenceEngine(Registry flags)
    {
        return new BookingPreferenceEngine(
            new List<IBookingPreferenceModifier>
            {
                new CapBookingPreferenceAdjuster(47441.0),
                new FlaggedBookingPreferenceAdjuster(flags, "booking-preference-pending", 7.7),
                new RoundingBookingPreferenceAdjuster(0.1),
                new LabelBookingPreferenceAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["central"] = 1.28 }),
                new MinimumBookingPreferenceAdjuster(39.8),
            },
            new List<string>(),
            4.7,
            23020.8);
    }

    public double Run(BookingPreference item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BookingPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
