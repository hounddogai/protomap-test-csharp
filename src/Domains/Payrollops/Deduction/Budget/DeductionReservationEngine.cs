namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction reservation calculation.</summary>
public interface IDeductionReservationAdjuster
{
    string Describe();
    double Adjust(DeductionReservation item, double valueValue);
}

public sealed record PercentDeductionReservationAdjuster(double Percent) : IDeductionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeductionReservationAdjuster(double Cap) : IDeductionReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDeductionReservationAdjuster(Registry Flags, string Flag, double Percent) : IDeductionReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeductionReservationAdjuster(List<double> Bounds, List<double> Rates) : IDeductionReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionReservation item, double valueValue)
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

/// <summary>Runs the deduction reservation steps in order and keeps a trace of each.</summary>
public sealed class DeductionReservationEngine(List<IDeductionReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionReservationEngine DefaultDeductionReservationEngine(Registry flags)
    {
        return new DeductionReservationEngine(
            new List<IDeductionReservationAdjuster>
            {
                new PercentDeductionReservationAdjuster(-10.9),
                new CapDeductionReservationAdjuster(46300.0),
                new FlaggedDeductionReservationAdjuster(flags, "deduction-reservation-fallback", -1.8),
                new TieredDeductionReservationAdjuster(
                    new List<double> { 243.0, 8055.0 },
                    new List<double> { 0.87, 0.81 }),
            },
            new List<string>(),
            2.0,
            24758.0);
    }

    public double Price(DeductionReservation item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
