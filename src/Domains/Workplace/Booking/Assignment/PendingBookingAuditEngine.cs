namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending booking audit calculation.</summary>
public interface IPendingBookingAuditModifier
{
    string Describe();
    double Adjust(PendingBookingAudit item, double valueValue);
}

public sealed record MinimumPendingBookingAuditAdjuster(double Minimum) : IPendingBookingAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingBookingAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingBookingAuditMaximumOffsetAdjuster(double Weight) : IPendingBookingAuditModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PendingBookingAudit item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedPendingBookingAuditAdjuster(Registry Flags, string Flag, double Percent) : IPendingBookingAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBookingAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPendingBookingAuditAdjuster(double Percent) : IPendingBookingAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBookingAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingBookingAuditAdjuster(List<double> Bounds, List<double> Rates) : IPendingBookingAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingBookingAudit item, double valueValue)
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

/// <summary>Runs the pending booking audit steps in order and keeps a trace of each.</summary>
public sealed class PendingBookingAuditEngine(List<IPendingBookingAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBookingAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBookingAuditEngine DefaultPendingBookingAuditEngine(Registry flags)
    {
        return new PendingBookingAuditEngine(
            new List<IPendingBookingAuditModifier>
            {
                new MinimumPendingBookingAuditAdjuster(23.4),
                new PendingBookingAuditMaximumOffsetAdjuster(0.63),
                new FlaggedPendingBookingAuditAdjuster(flags, "pending-booking-audit-scheduled", 1.3),
                new PercentPendingBookingAuditAdjuster(-8.8),
                new TieredPendingBookingAuditAdjuster(
                    new List<double> { 269.0, 3443.0, 5004.0, 5478.0 },
                    new List<double> { 1.07, 1.0, 1.09, 1.05 }),
            },
            new List<string>(),
            5.0,
            38104.6);
    }

    public double Run(PendingBookingAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingBookingAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
