namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending booking plan calculation.</summary>
public interface IPendingBookingPlanModifier
{
    string Describe();
    double Adjust(PendingBookingPlan item, double valueValue);
}

public sealed record MinimumPendingBookingPlanAdjuster(double Minimum) : IPendingBookingPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingBookingPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingBookingPlanAdjuster(List<double> Bounds, List<double> Rates) : IPendingBookingPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingBookingPlan item, double valueValue)
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

public sealed record PercentPendingBookingPlanAdjuster(double Percent) : IPendingBookingPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBookingPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingBookingPlanAdjuster(double Cap) : IPendingBookingPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBookingPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingBookingPlanAdjuster(Registry Flags, string Flag, double Percent) : IPendingBookingPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBookingPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending booking plan steps in order and keeps a trace of each.</summary>
public sealed class PendingBookingPlanEngine(List<IPendingBookingPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBookingPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBookingPlanEngine DefaultPendingBookingPlanEngine(Registry flags)
    {
        return new PendingBookingPlanEngine(
            new List<IPendingBookingPlanModifier>
            {
                new MinimumPendingBookingPlanAdjuster(39.3),
                new TieredPendingBookingPlanAdjuster(
                    new List<double> { 1273.0, 4278.0 },
                    new List<double> { 1.07, 0.89 }),
                new PercentPendingBookingPlanAdjuster(22.8),
                new CapPendingBookingPlanAdjuster(18258.0),
                new FlaggedPendingBookingPlanAdjuster(flags, "pending-booking-plan-rolling", 2.4),
            },
            new List<string>(),
            4.2,
            71772.8);
    }

    public double Run(PendingBookingPlan item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingBookingPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
