namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled access badge reservation calculation.</summary>
public interface IScheduledAccessBadgeReservationAdjuster
{
    string Describe();
    double Adjust(ScheduledAccessBadgeReservation item, double valueValue);
}

public sealed record FlaggedScheduledAccessBadgeReservationAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAccessBadgeReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAccessBadgeReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledAccessBadgeReservationAdjuster(List<double> Bounds, List<double> Rates) : IScheduledAccessBadgeReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledAccessBadgeReservation item, double valueValue)
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

public sealed record RoundingScheduledAccessBadgeReservationAdjuster(double Step) : IScheduledAccessBadgeReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAccessBadgeReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledAccessBadgeReservationAdjuster(double Minimum) : IScheduledAccessBadgeReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccessBadgeReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled access badge reservation steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccessBadgeReservationEngine(List<IScheduledAccessBadgeReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccessBadgeReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccessBadgeReservationEngine DefaultScheduledAccessBadgeReservationEngine(Registry flags)
    {
        return new ScheduledAccessBadgeReservationEngine(
            new List<IScheduledAccessBadgeReservationAdjuster>
            {
                new FlaggedScheduledAccessBadgeReservationAdjuster(
                    flags,
                    "scheduled-access-badge-reservation-fallback",
                    8.3),
                new TieredScheduledAccessBadgeReservationAdjuster(
                    new List<double> { 1765.0, 3467.0, 5967.0, 6834.0 },
                    new List<double> { 0.89, 1.01, 1.1, 1.11 }),
                new RoundingScheduledAccessBadgeReservationAdjuster(0.25),
                new MinimumScheduledAccessBadgeReservationAdjuster(27.4),
            },
            new List<string>(),
            2.4,
            57345.7);
    }

    public double Price(ScheduledAccessBadgeReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccessBadgeReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
