namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy booking audit calculation.</summary>
public interface ILegacyBookingAuditFactor
{
    string Describe();
    double Adjust(LegacyBookingAudit item, double valueValue);
}

public sealed record TitleLegacyBookingAuditAdjuster(Dictionary<string, double> Factors) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyBookingAuditAdjuster(double Minimum) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLegacyBookingAuditAdjuster(List<double> Bounds, List<double> Rates) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
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

public sealed record RoundingLegacyBookingAuditAdjuster(double Step) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacyBookingAuditSequenceOffsetAdjuster(double Weight) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedLegacyBookingAuditAdjuster(Registry Flags, string Flag, double Percent) : ILegacyBookingAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyBookingAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy booking audit steps in order and keeps a trace of each.</summary>
public sealed class LegacyBookingAuditEngine(List<ILegacyBookingAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBookingAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBookingAuditEngine DefaultLegacyBookingAuditEngine(Registry flags)
    {
        return new LegacyBookingAuditEngine(
            new List<ILegacyBookingAuditFactor>
            {
                new TitleLegacyBookingAuditAdjuster(new Dictionary<string, double> { ["central"] = 1.08 }),
                new MinimumLegacyBookingAuditAdjuster(23.0),
                new TieredLegacyBookingAuditAdjuster(
                    new List<double> { 710.0, 9556.0 },
                    new List<double> { 0.88, 0.91 }),
                new RoundingLegacyBookingAuditAdjuster(0.5),
                new LegacyBookingAuditSequenceOffsetAdjuster(1.28),
                new FlaggedLegacyBookingAuditAdjuster(flags, "legacy-booking-audit-seasonal", -1.4),
            },
            new List<string>(),
            1.6,
            55648.7);
    }

    public double Run(LegacyBookingAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBookingAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
