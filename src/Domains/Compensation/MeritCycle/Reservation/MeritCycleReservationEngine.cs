namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle reservation calculation.</summary>
public interface IMeritCycleReservationAdjuster
{
    string Describe();
    double Adjust(MeritCycleReservation item, double valueValue);
}

public sealed record MeritCycleReservationRetriesOffsetAdjuster(double Weight) : IMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(MeritCycleReservation item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredMeritCycleReservationAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleReservation item, double valueValue)
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

public sealed record RoundingMeritCycleReservationAdjuster(double Step) : IMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedMeritCycleReservationAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMeritCycleReservationAdjuster(double Minimum) : IMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle reservation steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleReservationEngine(List<IMeritCycleReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleReservationEngine DefaultMeritCycleReservationEngine(Registry flags)
    {
        return new MeritCycleReservationEngine(
            new List<IMeritCycleReservationAdjuster>
            {
                new MeritCycleReservationRetriesOffsetAdjuster(1.78),
                new TieredMeritCycleReservationAdjuster(
                    new List<double> { 4668.0, 5678.0, 8980.0 },
                    new List<double> { 1.12, 1.16, 1.08 }),
                new RoundingMeritCycleReservationAdjuster(0.25),
                new FlaggedMeritCycleReservationAdjuster(flags, "merit-cycle-reservation-tiered", 7.6),
                new MinimumMeritCycleReservationAdjuster(1.1),
            },
            new List<string>(),
            1.3,
            67988.6);
    }

    public double Run(MeritCycleReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
