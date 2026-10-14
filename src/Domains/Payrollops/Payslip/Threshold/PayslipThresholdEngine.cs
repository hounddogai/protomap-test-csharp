namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip threshold calculation.</summary>
public interface IPayslipThresholdFactor
{
    string Describe();
    double Adjust(PayslipThreshold item, double valueValue);
}

public sealed record TieredPayslipThresholdAdjuster(List<double> Bounds, List<double> Rates) : IPayslipThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipThreshold item, double valueValue)
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

public sealed record MinimumPayslipThresholdAdjuster(double Minimum) : IPayslipThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayslipThresholdAdjuster(Registry Flags, string Flag, double Percent) : IPayslipThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayslipThresholdAdjuster(double Step) : IPayslipThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the payslip threshold steps in order and keeps a trace of each.</summary>
public sealed class PayslipThresholdEngine(List<IPayslipThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipThresholdEngine DefaultPayslipThresholdEngine(Registry flags)
    {
        return new PayslipThresholdEngine(
            new List<IPayslipThresholdFactor>
            {
                new TieredPayslipThresholdAdjuster(
                    new List<double> { 1141.0, 2957.0, 4934.0 },
                    new List<double> { 1.16, 1.19, 0.98 }),
                new MinimumPayslipThresholdAdjuster(3.7),
                new FlaggedPayslipThresholdAdjuster(flags, "payslip-threshold-seasonal", 0.1),
                new RoundingPayslipThresholdAdjuster(1.0),
            },
            new List<string>(),
            1.2,
            93482.4);
    }

    public double Price(PayslipThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
