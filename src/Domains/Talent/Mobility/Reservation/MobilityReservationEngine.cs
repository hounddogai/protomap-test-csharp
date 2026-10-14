namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility reservation calculation.</summary>
public interface IMobilityReservationModifier
{
    string Describe();
    double Adjust(MobilityReservation item, double valueValue);
}

public sealed record FlaggedMobilityReservationAdjuster(Registry Flags, string Flag, double Percent) : IMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilityReservationAdjuster(double Cap) : IMobilityReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingMobilityReservationAdjuster(double Step) : IMobilityReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentMobilityReservationAdjuster(double Percent) : IMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MobilityReservationWindowDaysOffsetAdjuster(double Weight) : IMobilityReservationModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(MobilityReservation item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class MobilityReservationEngine(List<IMobilityReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityReservationEngine DefaultMobilityReservationEngine(Registry flags)
    {
        return new MobilityReservationEngine(
            new List<IMobilityReservationModifier>
            {
                new FlaggedMobilityReservationAdjuster(flags, "mobility-reservation-tiered", 1.4),
                new CapMobilityReservationAdjuster(36512.0),
                new RoundingMobilityReservationAdjuster(0.1),
                new PercentMobilityReservationAdjuster(-3.6),
                new MobilityReservationWindowDaysOffsetAdjuster(1.01),
            },
            new List<string>(),
            2.4,
            67285.9);
    }

    public double Compute(MobilityReservation item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
