namespace Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>One step of the batched payslip metric calculation.</summary>
public interface IBatchedPayslipMetricStep
{
    string Describe();
    double Adjust(BatchedPayslipMetric item, double valueValue);
}

public sealed record UnitBatchedPayslipMetricAdjuster(Dictionary<string, double> Factors) : IBatchedPayslipMetricStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(BatchedPayslipMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedPayslipMetricAdjuster(double Percent) : IBatchedPayslipMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPayslipMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedPayslipMetricAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPayslipMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPayslipMetric item, double valueValue)
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

/// <summary>Runs the batched payslip metric steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayslipMetricEngine(List<IBatchedPayslipMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayslipMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayslipMetricEngine DefaultBatchedPayslipMetricEngine()
    {
        return new BatchedPayslipMetricEngine(
            new List<IBatchedPayslipMetricStep>
            {
                new UnitBatchedPayslipMetricAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.12, ["online"] = 1.21 }),
                new PercentBatchedPayslipMetricAdjuster(1.3),
                new TieredBatchedPayslipMetricAdjuster(
                    new List<double> { 2434.0, 7634.0, 9117.0 },
                    new List<double> { 1.15, 1.12, 0.86 }),
            },
            new List<string>(),
            4.1,
            78117.6);
    }

    public double Evaluate(BatchedPayslipMetric item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayslipMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
