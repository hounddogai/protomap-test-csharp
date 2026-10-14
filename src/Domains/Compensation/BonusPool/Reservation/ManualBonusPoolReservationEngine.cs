namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual bonus pool reservation calculation.</summary>
public interface IManualBonusPoolReservationAdjuster
{
    string Describe();
    double Adjust(ManualBonusPoolReservation item, double valueValue);
}

public sealed record CapManualBonusPoolReservationAdjuster(double Cap) : IManualBonusPoolReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBonusPoolReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualBonusPoolReservationAdjuster(double Step) : IManualBonusPoolReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBonusPoolReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualBonusPoolReservationAdjuster(Registry Flags, string Flag, double Percent) : IManualBonusPoolReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualBonusPoolReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual bonus pool reservation steps in order and keeps a trace of each.</summary>
public sealed class ManualBonusPoolReservationEngine(List<IManualBonusPoolReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBonusPoolReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBonusPoolReservationEngine DefaultManualBonusPoolReservationEngine(Registry flags)
    {
        return new ManualBonusPoolReservationEngine(
            new List<IManualBonusPoolReservationAdjuster>
            {
                new CapManualBonusPoolReservationAdjuster(9253.0),
                new RoundingManualBonusPoolReservationAdjuster(0.25),
                new FlaggedManualBonusPoolReservationAdjuster(flags, "manual-bonus-pool-reservation-regional", 4.4),
            },
            new List<string>(),
            1.6,
            49859.9);
    }

    public double Evaluate(ManualBonusPoolReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBonusPoolReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
