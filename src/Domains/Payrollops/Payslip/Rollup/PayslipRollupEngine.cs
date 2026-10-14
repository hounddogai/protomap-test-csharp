namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip rollup calculation.</summary>
public interface IPayslipRollupModifier
{
    string Describe();
    double Adjust(PayslipRollup item, double valueValue);
}

public sealed record RoundingPayslipRollupAdjuster(double Step) : IPayslipRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitPayslipRollupAdjuster(Dictionary<string, double> Factors) : IPayslipRollupModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PayslipRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPayslipRollupAdjuster(Registry Flags, string Flag, double Percent) : IPayslipRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipRollupMaximumOffsetAdjuster(double Weight) : IPayslipRollupModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PayslipRollup item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record MinimumPayslipRollupAdjuster(double Minimum) : IPayslipRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip rollup steps in order and keeps a trace of each.</summary>
public sealed class PayslipRollupEngine(List<IPayslipRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipRollupEngine DefaultPayslipRollupEngine(Registry flags)
    {
        return new PayslipRollupEngine(
            new List<IPayslipRollupModifier>
            {
                new RoundingPayslipRollupAdjuster(1.0),
                new UnitPayslipRollupAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.11, ["retail"] = 1.21, ["online"] = 0.96 }),
                new FlaggedPayslipRollupAdjuster(flags, "payslip-rollup-tiered", -9.4),
                new PayslipRollupMaximumOffsetAdjuster(0.09),
                new MinimumPayslipRollupAdjuster(44.2),
            },
            new List<string>(),
            3.6,
            14739.7);
    }

    public double Run(PayslipRollup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
