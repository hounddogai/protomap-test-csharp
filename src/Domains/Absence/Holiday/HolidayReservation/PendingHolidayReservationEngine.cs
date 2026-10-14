namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending holiday reservation calculation.</summary>
public interface IPendingHolidayReservationAdjuster
{
    string Describe();
    double Adjust(PendingHolidayReservation item, double valueValue);
}

public sealed record PendingHolidayReservationExpiresAtOffsetAdjuster(double Weight) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingPendingHolidayReservationAdjuster(double Step) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPendingHolidayReservationAdjuster(Registry Flags, string Flag, double Percent) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingHolidayReservationAdjuster(double Minimum) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingHolidayReservationAdjuster(double Percent) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingHolidayReservationAdjuster(double Cap) : IPendingHolidayReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingHolidayReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pending holiday reservation steps in order and keeps a trace of each.</summary>
public sealed class PendingHolidayReservationEngine(List<IPendingHolidayReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingHolidayReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingHolidayReservationEngine DefaultPendingHolidayReservationEngine(Registry flags)
    {
        return new PendingHolidayReservationEngine(
            new List<IPendingHolidayReservationAdjuster>
            {
                new PendingHolidayReservationExpiresAtOffsetAdjuster(0.17),
                new RoundingPendingHolidayReservationAdjuster(1.0),
                new FlaggedPendingHolidayReservationAdjuster(flags, "pending-holiday-reservation-lenient", -4.2),
                new MinimumPendingHolidayReservationAdjuster(39.3),
                new PercentPendingHolidayReservationAdjuster(-13.5),
                new CapPendingHolidayReservationAdjuster(48221.0),
            },
            new List<string>(),
            3.6,
            31576.8);
    }

    public double Compute(PendingHolidayReservation item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingHolidayReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
