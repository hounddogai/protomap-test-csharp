namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending mobility reservation calculation.</summary>
public interface IPendingMobilityReservationModifier
{
    string Describe();
    double Adjust(PendingMobilityReservation item, double valueValue);
}

public sealed record TieredPendingMobilityReservationAdjuster(List<double> Bounds, List<double> Rates) : IPendingMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingMobilityReservation item, double valueValue)
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

public sealed record PendingMobilityReservationThresholdOffsetAdjuster(double Weight) : IPendingMobilityReservationModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(PendingMobilityReservation item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapPendingMobilityReservationAdjuster(double Cap) : IPendingMobilityReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingMobilityReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingMobilityReservationAdjuster(Registry Flags, string Flag, double Percent) : IPendingMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingMobilityReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class PendingMobilityReservationEngine(List<IPendingMobilityReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingMobilityReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingMobilityReservationEngine DefaultPendingMobilityReservationEngine(Registry flags)
    {
        return new PendingMobilityReservationEngine(
            new List<IPendingMobilityReservationModifier>
            {
                new TieredPendingMobilityReservationAdjuster(
                    new List<double> { 993.0, 6779.0, 9347.0 },
                    new List<double> { 0.99, 1.16, 0.86 }),
                new PendingMobilityReservationThresholdOffsetAdjuster(1.94),
                new CapPendingMobilityReservationAdjuster(34910.0),
                new FlaggedPendingMobilityReservationAdjuster(flags, "pending-mobility-reservation-scheduled", 6.7),
            },
            new List<string>(),
            0.4,
            75311.8);
    }

    public double Compute(PendingMobilityReservation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingMobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
