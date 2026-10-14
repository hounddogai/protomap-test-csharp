namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the payslip metric calculation.</summary>
public interface IPayslipMetricAdjuster
{
    string Describe();
    double Adjust(PayslipMetric item, double valueValue);
}

public sealed record NamePayslipMetricAdjuster(Dictionary<string, double> Factors) : IPayslipMetricAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayslipMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayslipMetricAdjuster(List<double> Bounds, List<double> Rates) : IPayslipMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipMetric item, double valueValue)
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

public sealed record CapPayslipMetricAdjuster(double Cap) : IPayslipMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayslipMetricAdjuster(Registry Flags, string Flag, double Percent) : IPayslipMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayslipMetricAdjuster(double Minimum) : IPayslipMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip metric steps in order and keeps a trace of each.</summary>
public sealed class PayslipMetricEngine(List<IPayslipMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipMetricEngine DefaultPayslipMetricEngine(Registry flags)
    {
        return new PayslipMetricEngine(
            new List<IPayslipMetricAdjuster>
            {
                new NamePayslipMetricAdjuster(new Dictionary<string, double> { ["north"] = 0.84 }),
                new TieredPayslipMetricAdjuster(
                    new List<double> { 1780.0, 7837.0, 9274.0 },
                    new List<double> { 1.09, 0.96, 0.9 }),
                new CapPayslipMetricAdjuster(16456.0),
                new FlaggedPayslipMetricAdjuster(flags, "payslip-metric-legacy", -7.9),
                new MinimumPayslipMetricAdjuster(46.3),
            },
            new List<string>(),
            2.3,
            40113.4);
    }

    public double Run(PayslipMetric item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
