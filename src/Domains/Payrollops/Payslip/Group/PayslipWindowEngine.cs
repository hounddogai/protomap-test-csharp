namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip window calculation.</summary>
public interface IPayslipWindowModifier
{
    string Describe();
    double Adjust(PayslipWindow item, double valueValue);
}

public sealed record PercentPayslipWindowAdjuster(double Percent) : IPayslipWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayslipWindowAdjuster(List<double> Bounds, List<double> Rates) : IPayslipWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipWindow item, double valueValue)
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

public sealed record RoundingPayslipWindowAdjuster(double Step) : IPayslipWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the payslip window steps in order and keeps a trace of each.</summary>
public sealed class PayslipWindowEngine(List<IPayslipWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipWindowEngine DefaultPayslipWindowEngine()
    {
        return new PayslipWindowEngine(
            new List<IPayslipWindowModifier>
            {
                new PercentPayslipWindowAdjuster(21.5),
                new TieredPayslipWindowAdjuster(
                    new List<double> { 3305.0, 3378.0, 6045.0, 8524.0 },
                    new List<double> { 0.85, 0.92, 1.0, 1.08 }),
                new RoundingPayslipWindowAdjuster(0.5),
            },
            new List<string>(),
            0.2,
            94180.2);
    }

    public double Price(PayslipWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
