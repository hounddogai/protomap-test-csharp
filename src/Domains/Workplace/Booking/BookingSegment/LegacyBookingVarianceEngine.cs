namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy booking variance calculation.</summary>
public interface ILegacyBookingVarianceModifier
{
    string Describe();
    double Adjust(LegacyBookingVariance item, double valueValue);
}

public sealed record TieredLegacyBookingVarianceAdjuster(List<double> Bounds, List<double> Rates) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
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

public sealed record FlaggedLegacyBookingVarianceAdjuster(Registry Flags, string Flag, double Percent) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyBookingVarianceAdjuster(double Percent) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyBookingVarianceAdjuster(double Step) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLegacyBookingVarianceAdjuster(double Minimum) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameLegacyBookingVarianceAdjuster(Dictionary<string, double> Factors) : ILegacyBookingVarianceModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacyBookingVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy booking variance steps in order and keeps a trace of each.</summary>
public sealed class LegacyBookingVarianceEngine(List<ILegacyBookingVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBookingVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBookingVarianceEngine DefaultLegacyBookingVarianceEngine(Registry flags)
    {
        return new LegacyBookingVarianceEngine(
            new List<ILegacyBookingVarianceModifier>
            {
                new TieredLegacyBookingVarianceAdjuster(
                    new List<double> { 106.0, 2855.0, 3437.0, 9807.0 },
                    new List<double> { 1.09, 0.87, 1.08, 1.1 }),
                new FlaggedLegacyBookingVarianceAdjuster(flags, "legacy-booking-variance-fallback", 9.9),
                new PercentLegacyBookingVarianceAdjuster(23.5),
                new RoundingLegacyBookingVarianceAdjuster(1.0),
                new MinimumLegacyBookingVarianceAdjuster(24.3),
                new NameLegacyBookingVarianceAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.0, ["north"] = 1.28 }),
            },
            new List<string>(),
            4.8,
            87744.2);
    }

    public double Evaluate(LegacyBookingVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBookingVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
