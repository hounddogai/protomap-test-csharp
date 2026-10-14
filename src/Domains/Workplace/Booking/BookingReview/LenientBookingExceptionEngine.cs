namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient booking exception calculation.</summary>
public interface ILenientBookingExceptionFactor
{
    string Describe();
    double Adjust(LenientBookingException item, double valueValue);
}

public sealed record MinimumLenientBookingExceptionAdjuster(double Minimum) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBookingException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceLenientBookingExceptionAdjuster(Dictionary<string, double> Factors) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientBookingException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientBookingExceptionAdjuster(List<double> Bounds, List<double> Rates) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientBookingException item, double valueValue)
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

public sealed record FlaggedLenientBookingExceptionAdjuster(Registry Flags, string Flag, double Percent) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientBookingException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientBookingExceptionAdjuster(double Step) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBookingException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LenientBookingExceptionAmountOffsetAdjuster(double Weight) : ILenientBookingExceptionFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientBookingException item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the lenient booking exception steps in order and keeps a trace of each.</summary>
public sealed class LenientBookingExceptionEngine(List<ILenientBookingExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBookingExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBookingExceptionEngine DefaultLenientBookingExceptionEngine(Registry flags)
    {
        return new LenientBookingExceptionEngine(
            new List<ILenientBookingExceptionFactor>
            {
                new MinimumLenientBookingExceptionAdjuster(41.1),
                new SourceLenientBookingExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["retail"] = 1.34 }),
                new TieredLenientBookingExceptionAdjuster(
                    new List<double> { 5916.0, 7366.0 },
                    new List<double> { 0.98, 1.15 }),
                new FlaggedLenientBookingExceptionAdjuster(flags, "lenient-booking-exception-weighted", 3.4),
                new RoundingLenientBookingExceptionAdjuster(1.0),
                new LenientBookingExceptionAmountOffsetAdjuster(1.23),
            },
            new List<string>(),
            1.6,
            37945.9);
    }

    public double Run(LenientBookingException item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LenientBookingException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
