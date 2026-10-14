namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip batch calculation.</summary>
public interface IPayslipBatchAdjuster
{
    string Describe();
    double Adjust(PayslipBatch item, double valueValue);
}

public sealed record CountryPayslipBatchAdjuster(Dictionary<string, double> Factors) : IPayslipBatchAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PayslipBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPayslipBatchAdjuster(double Step) : IPayslipBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPayslipBatchAdjuster(double Percent) : IPayslipBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayslipBatchAdjuster(double Cap) : IPayslipBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the payslip batch steps in order and keeps a trace of each.</summary>
public sealed class PayslipBatchEngine(List<IPayslipBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipBatchEngine DefaultPayslipBatchEngine()
    {
        return new PayslipBatchEngine(
            new List<IPayslipBatchAdjuster>
            {
                new CountryPayslipBatchAdjuster(new Dictionary<string, double> { ["north"] = 0.7, ["east"] = 0.77 }),
                new RoundingPayslipBatchAdjuster(5.0),
                new PercentPayslipBatchAdjuster(-22.4),
                new CapPayslipBatchAdjuster(38726.0),
            },
            new List<string>(),
            3.2,
            60784.0);
    }

    public double Run(PayslipBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<PayslipBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
