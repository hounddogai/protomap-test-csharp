namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the payslip rule calculation.</summary>
public interface IPayslipRuleAdjuster
{
    string Describe();
    double Adjust(PayslipRule item, double valueValue);
}

public sealed record MinimumPayslipRuleAdjuster(double Minimum) : IPayslipRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayslipRuleAdjuster(Registry Flags, string Flag, double Percent) : IPayslipRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipRuleAttemptsOffsetAdjuster(double Weight) : IPayslipRuleAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(PayslipRule item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record LabelPayslipRuleAdjuster(Dictionary<string, double> Factors) : IPayslipRuleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PayslipRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPayslipRuleAdjuster(double Cap) : IPayslipRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the payslip rule steps in order and keeps a trace of each.</summary>
public sealed class PayslipRuleEngine(List<IPayslipRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipRuleEngine DefaultPayslipRuleEngine(Registry flags)
    {
        return new PayslipRuleEngine(
            new List<IPayslipRuleAdjuster>
            {
                new MinimumPayslipRuleAdjuster(26.1),
                new FlaggedPayslipRuleAdjuster(flags, "payslip-rule-global", 8.3),
                new PayslipRuleAttemptsOffsetAdjuster(1.61),
                new LabelPayslipRuleAdjuster(new Dictionary<string, double> { ["retail"] = 0.91 }),
                new CapPayslipRuleAdjuster(41818.0),
            },
            new List<string>(),
            0.2,
            9004.7);
    }

    public double Evaluate(PayslipRule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
