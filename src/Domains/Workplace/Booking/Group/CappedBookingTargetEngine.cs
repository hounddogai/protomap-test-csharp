namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped booking target calculation.</summary>
public interface ICappedBookingTargetFactor
{
    string Describe();
    double Adjust(CappedBookingTarget item, double valueValue);
}

public sealed record FlaggedCappedBookingTargetAdjuster(Registry Flags, string Flag, double Percent) : ICappedBookingTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedBookingTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedBookingTargetAdjuster(List<double> Bounds, List<double> Rates) : ICappedBookingTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedBookingTarget item, double valueValue)
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

public sealed record CapCappedBookingTargetAdjuster(double Cap) : ICappedBookingTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBookingTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedBookingTargetAdjuster(double Step) : ICappedBookingTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedBookingTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedBookingTargetAdjuster(double Percent) : ICappedBookingTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedBookingTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped booking target steps in order and keeps a trace of each.</summary>
public sealed class CappedBookingTargetEngine(List<ICappedBookingTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBookingTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBookingTargetEngine DefaultCappedBookingTargetEngine(Registry flags)
    {
        return new CappedBookingTargetEngine(
            new List<ICappedBookingTargetFactor>
            {
                new FlaggedCappedBookingTargetAdjuster(flags, "capped-booking-target-legacy", -1.5),
                new TieredCappedBookingTargetAdjuster(
                    new List<double> { 381.0, 3576.0, 6368.0, 9293.0 },
                    new List<double> { 0.8, 0.85, 0.87, 1.06 }),
                new CapCappedBookingTargetAdjuster(37997.0),
                new RoundingCappedBookingTargetAdjuster(0.05),
                new PercentCappedBookingTargetAdjuster(-22.7),
            },
            new List<string>(),
            4.7,
            44245.1);
    }

    public double Evaluate(CappedBookingTarget item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedBookingTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
