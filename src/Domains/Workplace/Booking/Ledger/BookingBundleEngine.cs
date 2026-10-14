namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking bundle calculation.</summary>
public interface IBookingBundleFactor
{
    string Describe();
    double Adjust(BookingBundle item, double valueValue);
}

public sealed record MinimumBookingBundleAdjuster(double Minimum) : IBookingBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBookingBundleAdjuster(double Cap) : IBookingBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceBookingBundleAdjuster(Dictionary<string, double> Factors) : IBookingBundleFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBookingBundleAdjuster(double Step) : IBookingBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBookingBundleAdjuster(Registry Flags, string Flag, double Percent) : IBookingBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBookingBundleAdjuster(double Percent) : IBookingBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the booking bundle steps in order and keeps a trace of each.</summary>
public sealed class BookingBundleEngine(List<IBookingBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingBundleEngine DefaultBookingBundleEngine(Registry flags)
    {
        return new BookingBundleEngine(
            new List<IBookingBundleFactor>
            {
                new MinimumBookingBundleAdjuster(6.1),
                new CapBookingBundleAdjuster(8665.0),
                new SourceBookingBundleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.86, ["east"] = 0.81, ["online"] = 1.32 }),
                new RoundingBookingBundleAdjuster(1.0),
                new FlaggedBookingBundleAdjuster(flags, "booking-bundle-strict", -3.5),
                new PercentBookingBundleAdjuster(3.2),
            },
            new List<string>(),
            2.6,
            53534.5);
    }

    public double Compute(BookingBundle item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BookingBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
