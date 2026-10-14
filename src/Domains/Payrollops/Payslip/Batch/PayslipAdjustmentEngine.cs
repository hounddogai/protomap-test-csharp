namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip adjustment calculation.</summary>
public interface IPayslipAdjustmentModifier
{
    string Describe();
    double Adjust(PayslipAdjustment item, double valueValue);
}

public sealed record RoundingPayslipAdjustmentAdjuster(double Step) : IPayslipAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayslipAdjustmentScoreOffsetAdjuster(double Weight) : IPayslipAdjustmentModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PayslipAdjustment item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentPayslipAdjustmentAdjuster(double Percent) : IPayslipAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayslipAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IPayslipAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipAdjustment item, double valueValue)
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

public sealed record FlaggedPayslipAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IPayslipAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the payslip adjustment steps in order and keeps a trace of each.</summary>
public sealed class PayslipAdjustmentEngine(List<IPayslipAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipAdjustmentEngine DefaultPayslipAdjustmentEngine(Registry flags)
    {
        return new PayslipAdjustmentEngine(
            new List<IPayslipAdjustmentModifier>
            {
                new RoundingPayslipAdjustmentAdjuster(0.5),
                new PayslipAdjustmentScoreOffsetAdjuster(1.59),
                new PercentPayslipAdjustmentAdjuster(15.3),
                new TieredPayslipAdjustmentAdjuster(
                    new List<double> { 105.0, 7188.0 },
                    new List<double> { 0.97, 0.82 }),
                new FlaggedPayslipAdjustmentAdjuster(flags, "payslip-adjustment-capped", 0.2),
            },
            new List<string>(),
            1.8,
            28821.0);
    }

    public double Evaluate(PayslipAdjustment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
