namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance reservation calculation.</summary>
public interface IBalanceReservationStep
{
    string Describe();
    double Adjust(BalanceReservation item, double valueValue);
}

public sealed record MinimumBalanceReservationAdjuster(double Minimum) : IBalanceReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBalanceReservationAdjuster(Registry Flags, string Flag, double Percent) : IBalanceReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBalanceReservationAdjuster(double Cap) : IBalanceReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the balance reservation steps in order and keeps a trace of each.</summary>
public sealed class BalanceReservationEngine(List<IBalanceReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceReservationEngine DefaultBalanceReservationEngine(Registry flags)
    {
        return new BalanceReservationEngine(
            new List<IBalanceReservationStep>
            {
                new MinimumBalanceReservationAdjuster(33.2),
                new FlaggedBalanceReservationAdjuster(flags, "balance-reservation-fallback", 1.1),
                new CapBalanceReservationAdjuster(5193.0),
            },
            new List<string>(),
            4.5,
            65235.6);
    }

    public double Compute(BalanceReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
