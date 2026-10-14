namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>One step of the payslip budget calculation.</summary>
public interface IPayslipBudgetModifier
{
    string Describe();
    double Adjust(PayslipBudget item, double valueValue);
}

public sealed record TieredPayslipBudgetAdjuster(List<double> Bounds, List<double> Rates) : IPayslipBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipBudget item, double valueValue)
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

public sealed record PercentPayslipBudgetAdjuster(double Percent) : IPayslipBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayslipBudgetAdjuster(double Minimum) : IPayslipBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip budget steps in order and keeps a trace of each.</summary>
public sealed class PayslipBudgetEngine(List<IPayslipBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipBudgetEngine DefaultPayslipBudgetEngine()
    {
        return new PayslipBudgetEngine(
            new List<IPayslipBudgetModifier>
            {
                new TieredPayslipBudgetAdjuster(
                    new List<double> { 211.0, 1947.0, 8390.0 },
                    new List<double> { 0.96, 0.86, 0.93 }),
                new PercentPayslipBudgetAdjuster(-0.1),
                new MinimumPayslipBudgetAdjuster(2.0),
            },
            new List<string>(),
            0.7,
            78181.9);
    }

    public double Compute(PayslipBudget item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
