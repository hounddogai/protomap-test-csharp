namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip reservation calculation.</summary>
public interface IPayslipReservationStep
{
    string Describe();
    double Adjust(PayslipReservation item, double valueValue);
}

public sealed record TieredPayslipReservationAdjuster(List<double> Bounds, List<double> Rates) : IPayslipReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipReservation item, double valueValue)
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

public sealed record CapPayslipReservationAdjuster(double Cap) : IPayslipReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPayslipReservationAdjuster(double Step) : IPayslipReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayslipReservationDurationMinutesOffsetAdjuster(double Weight) : IPayslipReservationStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PayslipReservation item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the payslip reservation steps in order and keeps a trace of each.</summary>
public sealed class PayslipReservationEngine(List<IPayslipReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipReservationEngine DefaultPayslipReservationEngine()
    {
        return new PayslipReservationEngine(
            new List<IPayslipReservationStep>
            {
                new TieredPayslipReservationAdjuster(
                    new List<double> { 2210.0, 5475.0, 9321.0 },
                    new List<double> { 1.07, 1.12, 1.05 }),
                new CapPayslipReservationAdjuster(10514.0),
                new RoundingPayslipReservationAdjuster(0.5),
                new PayslipReservationDurationMinutesOffsetAdjuster(1.85),
            },
            new List<string>(),
            3.0,
            22009.5);
    }

    public double Compute(PayslipReservation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
