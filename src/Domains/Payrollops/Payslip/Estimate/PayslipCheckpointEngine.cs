namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>One step of the payslip checkpoint calculation.</summary>
public interface IPayslipCheckpointAdjuster
{
    string Describe();
    double Adjust(PayslipCheckpoint item, double valueValue);
}

public sealed record PercentPayslipCheckpointAdjuster(double Percent) : IPayslipCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipCheckpointCreatedAtOffsetAdjuster(double Weight) : IPayslipCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PayslipCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record MinimumPayslipCheckpointAdjuster(double Minimum) : IPayslipCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PayslipCheckpointEngine(List<IPayslipCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipCheckpointEngine DefaultPayslipCheckpointEngine()
    {
        return new PayslipCheckpointEngine(
            new List<IPayslipCheckpointAdjuster>
            {
                new PercentPayslipCheckpointAdjuster(7.1),
                new PayslipCheckpointCreatedAtOffsetAdjuster(1.33),
                new MinimumPayslipCheckpointAdjuster(42.3),
            },
            new List<string>(),
            2.4,
            91545.0);
    }

    public double Compute(PayslipCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
