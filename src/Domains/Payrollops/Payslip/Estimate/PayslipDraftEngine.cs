namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>One step of the payslip draft calculation.</summary>
public interface IPayslipDraftAdjuster
{
    string Describe();
    double Adjust(PayslipDraft item, double valueValue);
}

public sealed record CapPayslipDraftAdjuster(double Cap) : IPayslipDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PayslipDraftRatingOffsetAdjuster(double Weight) : IPayslipDraftAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(PayslipDraft item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record PercentPayslipDraftAdjuster(double Percent) : IPayslipDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayslipDraftAdjuster(double Minimum) : IPayslipDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip draft steps in order and keeps a trace of each.</summary>
public sealed class PayslipDraftEngine(List<IPayslipDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipDraftEngine DefaultPayslipDraftEngine()
    {
        return new PayslipDraftEngine(
            new List<IPayslipDraftAdjuster>
            {
                new CapPayslipDraftAdjuster(3812.0),
                new PayslipDraftRatingOffsetAdjuster(0.31),
                new PercentPayslipDraftAdjuster(-12.3),
                new MinimumPayslipDraftAdjuster(3.4),
            },
            new List<string>(),
            1.8,
            84849.3);
    }

    public double Run(PayslipDraft item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
